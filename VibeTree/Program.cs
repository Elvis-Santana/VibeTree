using FluentValidation;
using JasperFx;
using JasperFx.CodeGeneration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VibeTree.Features.Link.CreateLink;
using VibeTree.Features.Link.CreateLink.Commands;
using VibeTree.Features.Perfil;
using VibeTree.Features.Perfil.Get.GetById;
using VibeTree.Features.Perfil.Get.GetBySlug;
using VibeTree.Features.User;
using VibeTree.Features.User.CreateUser.Commands;
using VibeTree.Features.User.DeleteUser.Commands;
using VibeTree.Features.User.GetUser.GetByIdUser;
using VibeTree.Features.User.Login;
using VibeTree.Features.User.UpdateUser.Commands;
using VibeTree.Shared.Auth;
using VibeTree.Shared.Common;
using VibeTree.Shared.DbAppContext;
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


string connectionRead = builder.Configuration.GetConnectionString("ReadDb") ?? throw new ArgumentException();

builder.Services.AddDbContext<ReadDbContext>(optionsAction =>
    optionsAction.UseMySql(connectionRead, ServerVersion.AutoDetect(connectionRead))
);

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



builder.Host.UseWolverine(opts =>
{
    opts.Policies.UseDurableLocalQueues();
    opts.Policies.AutoApplyIdempotencyOnNonTransactionalHandlers();

    opts.Discovery.IncludeAssembly(typeof(CreateUserAndProfileCommand).Assembly);
    opts.Discovery.IncludeAssembly(typeof(DeleteUserCommand).Assembly);
    opts.Discovery.IncludeAssembly(typeof(UpdateUserCommand).Assembly);

    opts.CodeGeneration.AlwaysUseServiceLocationFor<WriteDbContext>();
    opts.CodeGeneration.AlwaysUseServiceLocationFor<ReadDbContext>();


    opts.Policies.ConfigureConventionalLocalRouting();

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

ConfigJWT(builder.Services, builder.Configuration);

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IValidator<LoginQuery>, LoginValidator>();
builder.Services.AddScoped<IValidator<CreateUserAndProfileCommand>, CreateUserAndProfileValidator>();
builder.Services.AddScoped<IValidator<UpdateUserCommand>, UpdateUserValidator>();
builder.Services.AddScoped<IValidator<DeleteUserCommand>, DeleteUserValidator>();
builder.Services.AddScoped<IValidator<CreateLinkCommand>, CreateLinkValidator>();


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

app.MapPost("/link", async (IMessageBus bus, [FromBody] CreateLinkCommand createLinkCommand) => {

    Result<LinkResponse> result = await bus.InvokeAsync<Result<LinkResponse>>(createLinkCommand);

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


static void ConfigJWT(IServiceCollection services, IConfiguration configuration)
{
    var jwt = configuration.GetSection("JwtSettings");
    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["secret_key"]));

    services.AddAuthentication("Bearer").AddJwtBearer(options =>
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            //ValidAudience = jwt[Audience],
            IssuerSigningKey = secretKey
        }
    );

    services.AddAuthorization();


}
public partial class Program { }