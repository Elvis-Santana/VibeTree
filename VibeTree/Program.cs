using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection.Metadata;
using VibeTree.Application.Auth;
using VibeTree.Application.ConfigureApplication;
using VibeTree.Application.Interfaces;
using VibeTree.Application.User;
using VibeTree.Application.User.Get;
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

builder.Services.AddTransient<IHandler<CreateUserCommand, UserResponse>, CreateUserHandler>();

builder.Services.AddTransient<IHandler<GetAllUserQuery, List<UserResponse>>, GetAllUserHandler>();

builder.Services.AddScoped<ITokenService, TokenService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
var forecast = Enumerable.Range(1, 5).Select(index =>
    new WeatherForecast
    (
        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        Random.Shared.Next(-20, 55),
        summaries[Random.Shared.Next(summaries.Length)]
    ))
    .ToArray();
return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}