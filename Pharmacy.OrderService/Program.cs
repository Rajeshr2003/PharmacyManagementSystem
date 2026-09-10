using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pharmacy.OrderService.Data;
using System.Text;
using Microsoft.OpenApi.Models;
using Pharmacy.OrderService.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Key"]!))
            };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpClient();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();





// using Microsoft.EntityFrameworkCore;
// using Pharmacy.OrderService.Data;

// var builder = WebApplication.CreateBuilder(args);

// // Add services
// builder.Services.AddControllers();

// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// builder.Services.AddDbContext<OrdersDbContext>(options =>
//     options.UseSqlServer(
//         builder.Configuration.GetConnectionString("DefaultConnection")));

// var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();

// app.MapControllers();

// app.Run();



// // using Microsoft.EntityFrameworkCore;
// // using Pharmacy.OrderService.Data;

// // var builder = WebApplication.CreateBuilder(args);

// // // Add services
// // builder.Services.AddControllers();

// // builder.Services.AddEndpointsApiExplorer();
// // builder.Services.AddOpenApi();

// // builder.Services.AddDbContext<OrdersDbContext>(options =>
// //     options.UseSqlServer(
// //         builder.Configuration.GetConnectionString("DefaultConnection")));

// // var app = builder.Build();

// // // Configure pipeline
// // if (app.Environment.IsDevelopment())
// // {
// //     app.MapOpenApi();
// // }

// // app.UseHttpsRedirection();

// // app.MapControllers();

// // app.Run();






// // // var builder = WebApplication.CreateBuilder(args);

// // // // Add services to the container.
// // // // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// // // builder.Services.AddOpenApi();

// // // var app = builder.Build();

// // // // Configure the HTTP request pipeline.
// // // if (app.Environment.IsDevelopment())
// // // {
// // //     app.MapOpenApi();
// // // }

// // // app.UseHttpsRedirection();

// // // var summaries = new[]
// // // {
// // //     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// // // };

// // // app.MapGet("/weatherforecast", () =>
// // // {
// // //     var forecast =  Enumerable.Range(1, 5).Select(index =>
// // //         new WeatherForecast
// // //         (
// // //             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
// // //             Random.Shared.Next(-20, 55),
// // //             summaries[Random.Shared.Next(summaries.Length)]
// // //         ))
// // //         .ToArray();
// // //     return forecast;
// // // })
// // // .WithName("GetWeatherForecast");

// // // app.Run();

// // // record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// // // {
// // //     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// // // }
