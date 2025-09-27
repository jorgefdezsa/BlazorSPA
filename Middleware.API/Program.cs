using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Middleware.API.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuración de opciones desde appsettings.json
var configuration = builder.Configuration;
builder.Services.Configure<ServiceBusOptions>(configuration.GetSection("ServiceBus"));

var tenantId = configuration["AzureAd:TenantId"];
var audience = configuration["AzureAd:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidAudience = audience,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7252") // ← puerto de tu Blazor SPA
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Middleware.API v1");
        options.RoutePrefix = string.Empty;
    });
}

// Middleware general
app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    if (!string.IsNullOrEmpty(authHeader))
    {
        Console.WriteLine($"🔍 Authorization Header recibido: {authHeader}");
    }
    else
    {
        Console.WriteLine("⚠️ No se recibió cabecera Authorization.");
    }

    await next();
});


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
