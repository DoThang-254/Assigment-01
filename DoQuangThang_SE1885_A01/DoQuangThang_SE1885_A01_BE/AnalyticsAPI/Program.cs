using AnalyticsAPI.Middlewares;
using BusinessLogic.Services.Implementations;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Implementations;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<FunewsManagementContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFE",
        policy =>
        {
            policy
                .WithOrigins("https://localhost:7293") // FE của bạn
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("=== AUTHENTICATION FAILED ===");
            Console.WriteLine(context.Exception.Message); // In ra lý do tại sao lỗi
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("=== TOKEN VALIDATED SUCCESS ==="); // In ra nếu thành công
            return Task.CompletedTask;
        }
    };
});

// 2. Đăng ký Repository
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

// 3. Đăng ký Service
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowFE");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
