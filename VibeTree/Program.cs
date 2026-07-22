using JasperFx;
using JasperFx.CodeGeneration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VibeTree.Application.Common;
using VibeTree.Application.ConfigureApplication;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Perfil;
using VibeTree.Application.Perfil.Get.GetById;
using VibeTree.Application.Perfil.Get.GetBySlug;
using VibeTree.Application.Sync;
using VibeTree.Application.User;
using VibeTree.Application.User.Create.Commands;
using VibeTree.Application.User.Delete.Commands;
using VibeTree.Application.User.Get.ById;
using VibeTree.Application.User.Login;
using VibeTree.Application.User.Update.Commands;
using VibeTree.Domain.Entity;
using VibeTree.Infrastructure.AppDbContext;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddOpenApi();

string connectionWrite = builder.Configuration.GetConnectionString("WriteDb") ?? throw new ArgumentException();

builder.Services.AddDbContext<WriteDbContext>(optionsAction =>
    optionsAction.UseMySql(connectionWrite, ServerVersion.AutoDetect(connectionWrite))
);

builder.Services.AddScoped<IWriteDbContext, WriteDbContext>();

string connectionRead = builder.Configuration.GetConnectionString("ReadDb") ?? throw new ArgumentException();

builder.Services.AddDbContext<ReadDbContext>(optionsAction =>
    optionsAction.UseMySql(connectionRead, ServerVersion.AutoDetect(connectionRead))
);
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


builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(CreateUserAndProfileCommand).Assembly);
    opts.Discovery.IncludeAssembly(typeof(DeleteUserCommand).Assembly);
    opts.Discovery.IncludeAssembly(typeof(UpdateUserCommand).Assembly);

    opts.CodeGeneration.AlwaysUseServiceLocationFor<IWriteDbContext>();
    opts.CodeGeneration.AlwaysUseServiceLocationFor<IReadDbContext>();

    if (builder.Environment.IsDevelopment())
    {
        opts.UseRuntimeCompilation();
    }
});

if (!builder.Environment.IsDevelopment())
{
    builder.Services.CritterStackDefaults(opts =>
    {
        opts.Production.GeneratedCodeMode = TypeLoadMode.Static;
        opts.Production.AssertAllPreGeneratedTypesExist = true;
    });
}





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


app.MapGet("/perfil/{id}",async(IMessageBus bus, string id) =>
{
    Result<PerfilResponse> result = await bus.InvokeAsync<Result<PerfilResponse>>(new GetPerfilByIdQuery(id));

    if (!result.IsSuccess)
        return Results.NotFound(result);
    return Results.Ok(result);
});

app.MapGet("/perfil/@{slug}", async (IMessageBus bus, string slug) =>
{
    Result<PerfilResponse> result = await bus.InvokeAsync<Result<PerfilResponse>> (new GetPerfilBySlugQuery(slug));

    if (!result.IsSuccess)
        return Results.NotFound(result);
    return Results.Ok(result);
});


app.MapPost("/user", async (IMessageBus bus, [FromBody] CreateUserAndProfileCommand createUserCommand) => {

        Result<Userlogin> result = await bus.InvokeAsync<Result<Userlogin>>(createUserCommand);


        if (!result.IsSuccess)
            return Results.BadRequest(result);


        return Results.Ok(result);
});

app.MapPatch("/user", async (IMessageBus bus,[FromBody] UpdateUserCommand updateUserCommand) => {

        Result<Userlogin> result = await bus.InvokeAsync<Result<Userlogin>>(updateUserCommand);

        if (!result.IsSuccess)
            return Results.BadRequest(result);


        return Results.Ok(result);
 });

app.MapDelete("/user/{id}", async (IMessageBus bus,string id) => {

    Result<bool> result = await bus.InvokeAsync<Result<bool>>(new DeleteUserCommand(id));

    if (!result.IsSuccess)
        return Results.BadRequest(result);


    return Results.Ok(result);
});



app.MapPost("/auth/login", async (IMessageBus bus, [FromBody] LoginQuery loginQuery) => {

        Result<Userlogin> result = await bus.InvokeAsync<Result<Userlogin>>(loginQuery);

    if (!result.IsSuccess)
        return Results.BadRequest(result);


    return Results.Ok(result);

});

app.MapGet("/user/me", async(IMessageBus bus, ClaimsPrincipal user ) =>
{
    var usuarioId =  user.FindFirstValue(ClaimTypes.NameIdentifier); ;

    var result = await bus.InvokeAsync<Result<UserResponse>>(new GetByIdUserQuery(usuarioId));

    if (!result.IsSuccess)
        return Results.NotFound(result);


    return Results.Ok(result);

}).RequireAuthorization();




return await app.RunJasperFxCommands(args);

public partial class Program { }