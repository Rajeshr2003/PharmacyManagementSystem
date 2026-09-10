using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pharmacy.AuthService.Configurations;
using Pharmacy.AuthService.Data;
using Pharmacy.AuthService.Interfaces;
using Pharmacy.AuthService.Middleware;
using Pharmacy.AuthService.Repositories;
using Pharmacy.AuthService.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Email Settings
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Dependency Injection
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<JwtService>();

// JWT Authentication
builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!))
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();




// using System.Net;
// using System.Text.Json;

// namespace Pharmacy.AuthService.Middleware;

// public class GlobalExceptionMiddleware
// {
//     private readonly RequestDelegate _next;

//     public GlobalExceptionMiddleware(RequestDelegate next)
//     {
//         _next = next;
//     }

//     public async Task InvokeAsync(HttpContext context)
//     {
//         try
//         {
//             await _next(context);
//         }
//         catch (Exception ex)
//         {
//             context.Response.ContentType = "application/json";

//             context.Response.StatusCode =
//                 (int)HttpStatusCode.InternalServerError;

//             var response = new
//             {
//                 StatusCode = context.Response.StatusCode,
//                 Message = ex.Message
//             };

//             await context.Response.WriteAsync(
//                 JsonSerializer.Serialize(response));
//         }
//     }
// }