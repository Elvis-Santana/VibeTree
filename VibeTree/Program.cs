using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VibeTree.Application.ConfigureApplication;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Result;
using VibeTree.Application.User;
using VibeTree.Application.User.Login;
using VibeTree.Infrastructure.AppDbContext;
using VibeTree.User.CreateUser;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(optionsAction =>
{
    string connection = builder.Configuration
    .GetConnectionString("DefaultConnection") ?? throw new ArgumentException();

    optionsAction.UseMySql(connection, ServerVersion.AutoDetect(connection));

});

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



app.MapPost("/user/create", async ([FromServices] IHandler<CreateUserCommand, Result<Userlogin>> handler,
    [FromBody] CreateUserCommand createUserCommand) => {

        Result<Userlogin> result = await handler.HandleAsync(createUserCommand);

        if (!result.IsSuccess)
            return Results.BadRequest(result);


        return Results.Ok(result);
    });



app.MapPost("/auth/login", async ([FromServices] IHandler<LoginQuery, Result<Userlogin>> handler,
    [FromBody] LoginQuery loginQuery) =>
{

    Result<Userlogin> result = await handler.HandleAsync(loginQuery);

    if (!result.IsSuccess)
        return Results.NotFound(result);


    return Results.Ok(result);

});

app.MapGet("auth/me", async() =>
{
    return Results.Ok(new { valid=true });
}).RequireAuthorization(); 




app.Run();

