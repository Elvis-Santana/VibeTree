using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VibeTree.Application.Auth.ValueObject;
using VibeTree.Application.ConfigureApplication;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Perfil;
using VibeTree.Application.Perfil.Create;
using VibeTree.Application.Perfil.Get.GetById;
using VibeTree.Application.Perfil.Get.GetBySlug;
using VibeTree.Application.Result;
using VibeTree.Application.Sync;
using VibeTree.Application.User;
using VibeTree.Application.User.Delete;
using VibeTree.Application.User.Get.ById;
using VibeTree.Application.User.Login;
using VibeTree.Application.User.Update;
using VibeTree.Domain.Entity;
using VibeTree.Infrastructure.AppDbContext;
using VibeTree.Infrastructure.Mediator;
using VibeTree.Infrastructure.Query;
using VibeTree.Infrastructure.Worker;
using VibeTree.User.CreateUser;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddOpenApi();
builder.Services.AddDbContext<WriteDbContext>(optionsAction =>
{
    string connection = builder.Configuration
    .GetConnectionString("WriteDb") ?? throw new ArgumentException();

    optionsAction.UseMySql(connection, ServerVersion.AutoDetect(connection));
   

});
builder.Services.AddDbContext<ReadDbContext>(optionsAction =>
{
    string connection = builder.Configuration
    .GetConnectionString("ReadDb") ?? throw new ArgumentException();

    optionsAction.UseMySql(connection, ServerVersion.AutoDetect(connection));

});

builder.Services.AddScoped<IWriteDbContext, WriteDbContext>();

builder.Services.AddScoped<IReadDbContext, ReadDbContext>();

builder.Services.AddOpenApi(optionsAction =>
{
    optionsAction.AddDocumentTransformer((document, contex, CancellationToken) =>
    {
        var requirements = new Dictionary<string, OpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT"
            }
        };


        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = requirements;

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =new  OpenApiReference{
                        Id="Bearer",
                        Type = ReferenceType.SecurityScheme

                    }
                },
                Array.Empty<string>()
            }
        });

        return Task.CompletedTask;
    });
  
});

builder.Services.ConfigureServicesApplication(builder.Configuration);


builder.Services.AddHostedService<WorkerSynchronizeUserDb>();
builder.Services.AddHostedService<WorkerSynchronizePerfilDb>();
builder.Services.AddSingleton<IQueueSynchronizeDb<SyncData<User>>, QueueSynchronize<User>>();
builder.Services.AddSingleton<IQueueSynchronizeDb<SyncData<Perfil>>, QueueSynchronize<Perfil>>();
builder.Services.AddScoped<IMediator, Mediator>();

const string policy = "_myAllowSpecificOrigins";

builder.Services.AddCors(op =>
{
    op.AddPolicy(policy, (x) =>
    {
        x.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin();

    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRouting();

app.UseCors(policy);
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();


app.MapGet("/perfil/{id}",async([FromServices] IMediator mediator,string id) =>
{
    Result<PerfilResponse> result = await mediator.SendAsync(new GetPerfilByIdQuery(id));

    if (!result.IsSuccess)
        return Results.NotFound(result);
    return Results.Ok(result);
});

app.MapGet("/perfil/@{slug}", async ([FromServices] IMediator mediator, string slug) =>
{
    Result<PerfilResponse> result = await mediator.SendAsync(new GetPerfilBySlugQuery(slug));

    if (!result.IsSuccess)
        return Results.NotFound(result);
    return Results.Ok(result);
});

app.MapPost("/perfil", async ([FromServices] IMediator mediator,
     [FromBody] CreatePerfilCommand command) =>
{

    Result<PerfilResponse> result = await mediator.SendAsync(command);

    if (!result.IsSuccess)
        return Results.BadRequest(result);


    return Results.Ok(result);
});

app.MapPost("/user", async ([FromServices] IMediator mediator,
    [FromBody] CreateUserAndProfileCommand createUserCommand) => {

        Result<Userlogin> result = await mediator.SendAsync(createUserCommand);

        if (!result.IsSuccess)
            return Results.BadRequest(result);


        return Results.Ok(result);
});

app.MapPatch("/user", async ([FromServices] IMediator mediator,
   [FromBody] UpdateUserCommand updateUserCommand) => {

        Result<Userlogin> result = await mediator.SendAsync(updateUserCommand);

        if (!result.IsSuccess)
            return Results.BadRequest(result);


        return Results.Ok(result);
 });

app.MapDelete("/user", async ([FromServices] IMediator mediator,
    [FromBody] DeleteUserCommand deleteUserCommand) => {

        Result<bool> result = await mediator.SendAsync(deleteUserCommand);

        if (!result.IsSuccess)
            return Results.BadRequest(result);


        return Results.Ok(result);
});



app.MapPost("/auth/login", async (
    [FromServices] IMediator mediator,
    [FromBody] LoginQuery loginQuery) => {

    var result = await mediator.SendAsync(loginQuery);

    if (!result.IsSuccess)
        return Results.BadRequest(result);


    return Results.Ok(result);

});

app.MapGet("/user/me", async( [FromServices] IMediator mediator, ClaimsPrincipal user ) =>
{
    var usuarioId =  user.FindFirstValue(ClaimTypes.NameIdentifier); ;

    GetByIdUserQuery getByIdUserQuery = new GetByIdUserQuery(usuarioId);

    var result = await mediator.SendAsync(getByIdUserQuery);

    if (!result.IsSuccess)
        return Results.NotFound(result);


    return Results.Ok(result);

}).RequireAuthorization(); 




app.Run();

public partial class Program { }