using BusinessLogic.Dto;
using BusinessLogic.Services.Implementations;
using BusinessLogic.Services.Interfaces;
using CoreAPI.Hubs;
using CoreAPI.Middlewares;
using DataAccess.DAO;
using DataAccess.Models;
using DataAccess.Models.Dto;
using DataAccess.Repositories.Implementations;
using DataAccess.Repositories.Interfaces;
using DataAccess.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using Presentation.ViewModels.Auth;
using Slot8_9_7_CsvHelper;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFE",
        policy =>
        {
            policy
                .WithOrigins("https://localhost:7293") // FE của bạn
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cấu hình DbContext với SQL Server
builder.Services.AddDbContext<FunewsManagementContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// Cấu hình SignalR
builder.Services.AddSignalR();

// Cấu hình OData EDM
var edmBuilder = new ODataConventionModelBuilder();

edmBuilder.EntitySet<NewsArticle>("news");
edmBuilder.EntityType<NewsArticle>().HasKey(n => n.NewsArticleId);
edmBuilder.EntitySet<Category>("category");
edmBuilder.EntityType<Category>().HasKey(c => c.CategoryId);
edmBuilder.EntitySet<SystemAccount>("account");
edmBuilder.EntityType<SystemAccount>().HasKey(a => a.AccountId);
edmBuilder.EntitySet<Tag>("tag");
edmBuilder.EntityType<Tag>().HasKey(t => t.TagId);
edmBuilder.EntitySet<CategoryArticleCount>("CountNewsByCategory");
edmBuilder.EntityType<CategoryArticleCount>().HasKey(c => c.CategoryId);
edmBuilder.EntitySet<ReportDTO>("report");

edmBuilder.EntityType<ReportDTO>()
    .HasKey(r => r.ReportId); // Id giả, chỉ để OData track

// Function: report by category
edmBuilder.EntityType<ReportDTO>()
    .Collection
    .Function("ByCategory")
    .ReturnsCollectionFromEntitySet<ReportDTO>("report")
    .Parameter<DateTime?>("fromDate");
edmBuilder.EntityType<ReportDTO>()
    .Collection
    .Function("ByCategory")
    .ReturnsCollectionFromEntitySet<ReportDTO>("report")
    .Parameter<DateTime?>("toDate");

// Function: report by author
edmBuilder.EntityType<ReportDTO>()
    .Collection
    .Function("ByAuthor")
    .ReturnsCollectionFromEntitySet<ReportDTO>("report")
    .Parameter<DateTime?>("fromDate");
edmBuilder.EntityType<ReportDTO>()
    .Collection
    .Function("ByAuthor")
    .ReturnsCollectionFromEntitySet<ReportDTO>("report")
    .Parameter<DateTime?>("toDate");

// Function: report by status
edmBuilder.EntityType<ReportDTO>()
    .Collection
    .Function("ByStatus")
    .ReturnsCollectionFromEntitySet<ReportDTO>("report")
    .Parameter<DateTime?>("fromDate");
edmBuilder.EntityType<ReportDTO>()
    .Collection
    .Function("ByStatus")
    .ReturnsCollectionFromEntitySet<ReportDTO>("report")
    .Parameter<DateTime?>("toDate");

// Function: summary
edmBuilder.EntityType<ReportDTO>()
    .Collection
    .Function("Summary")
    .Returns<ReportDTO>()
    .Parameter<DateTime?>("fromDate");
edmBuilder.EntityType<ReportDTO>()
    .Collection
    .Function("Summary")
    .Returns<ReportDTO>()
    .Parameter<DateTime?>("toDate");

// Đăng ký DI
builder.Services.AddScoped<INewsArticleService, NewsArticleService>();
builder.Services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
builder.Services.AddScoped<ISystemAccountRepository, SystemAccountRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITagService , TagService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISystemAccountService, SystemAccountService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshRepository>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Cấu hình AdminAccount từ appsettings.json
builder.Services.Configure<AdminAccount>(
    builder.Configuration.GetSection("AdminAccount")
);

// Cấu hình JwtSettings từ appsettings.json
builder.Services.AddHttpContextAccessor();


// Cấu hình controllers với OData và CSV formatter
builder.Services.AddControllers(options =>
    {
        // Register CSV output formatter so controllers can return CSV (text/csv)
        options.OutputFormatters.Add(new CsvOutputFormatter());
    })
    .AddOData(opt => opt
        .Select()
        .Filter()
        .OrderBy()
        .Count()
        .Expand() 
        .SetMaxTop(100).AddRouteComponents("api", edmBuilder.GetEdmModel()) 
    ).AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Cấu hình session
//builder.Services.AddDistributedMemoryCache(); // Cache lưu trữ dữ liệu session
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//});

// Cấu hình JWT Authentication
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

builder.Services.AddResponseCaching();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseSession();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowFE");

app.UseStaticFiles();

app.UseResponseCaching();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<AuditMiddleware>();

app.MapControllers();

app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
