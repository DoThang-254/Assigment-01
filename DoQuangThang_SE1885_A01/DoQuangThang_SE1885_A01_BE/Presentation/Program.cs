using BusinessLogic.Dto;
using BusinessLogic.Services.Implementations;
using BusinessLogic.Services.Interfaces;
using DataAccess.DAO;
using DataAccess.Models;
using DataAccess.Models.Dto;
using DataAccess.Repositories.Implementations;
using DataAccess.Repositories.Interfaces;
using DataAccess.Repository;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using Presentation.ViewModels.Auth;
using Slot8_9_7_CsvHelper;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<FunewsManagementContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

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
builder.Services.Configure<AdminAccount>(
    builder.Configuration.GetSection("AdminAccount")
);
builder.Services.AddHttpContextAccessor();

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
builder.Services.AddDistributedMemoryCache(); // Cache lưu trữ dữ liệu session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSession();

app.UseHttpsRedirection();

app.UseCors("AllowFE");

app.UseAuthorization();

app.MapControllers();

app.Run();
