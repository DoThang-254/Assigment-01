using DoQuangThang_SE1885_A01_FE.Handlers;
using DoQuangThang_SE1885_A01_FE.Hubs;
using DoQuangThang_SE1885_A01_FE.Middlewares;
using DoQuangThang_SE1885_A01_FE.Services;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVICES CONFIGURATION ---
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache(); // Cache trong RAM để truy xuất cực nhanh
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSignalR();

// Đăng ký Handlers và Worker
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddTransient<OfflineHandler>();
builder.Services.AddHostedService<CacheRefreshService>();

var overallTimeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(900));
var perTryTimeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(400));
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .Or<TimeoutRejectedException>()
    .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromMilliseconds(50));

var resilientStrategy = Policy.WrapAsync(overallTimeoutPolicy, retryPolicy, perTryTimeoutPolicy);

// THÊM MỚI: Cấu hình nới lỏng dành riêng cho AI API (Cho phép chờ tối đa 15 giây)
var aiTimeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(15));

void ConfigureClient(string name, string url, IAsyncPolicy<HttpResponseMessage> policy, bool useOffline = true)
{
    var clientBuilder = builder.Services.AddHttpClient(name, client =>
    {
        client.BaseAddress = new Uri(url);
    })
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .AddPolicyHandler(policy); // <-- Sử dụng policy truyền vào

    if (useOffline)
        clientBuilder.AddHttpMessageHandler<OfflineHandler>();
}

ConfigureClient("NewsAPI", "https://localhost:7066/", resilientStrategy);
ConfigureClient("AnalyticsAPI", "https://localhost:7078/", resilientStrategy);

ConfigureClient("AIAPI", "https://localhost:7150/", aiTimeoutPolicy, useOffline: false);

// Worker Clients
builder.Services.AddHttpClient("CoreWorkerClient", c => c.BaseAddress = new Uri("https://localhost:7066/")).AddPolicyHandler(resilientStrategy);
builder.Services.AddHttpClient("AnalyticsWorkerClient", c => c.BaseAddress = new Uri("https://localhost:7078/")).AddPolicyHandler(resilientStrategy);

var app = builder.Build();

// --- 4. MIDDLEWARE PIPELINE ---
if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error");
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<ReportHub>("/reportHub");

app.Run();


//using DoQuangThang_SE1885_A01_FE.Handlers;
//using DoQuangThang_SE1885_A01_FE.Hubs;
//using DoQuangThang_SE1885_A01_FE.Services;
//using Polly;
//using Polly.Extensions.Http;
//using Polly.Timeout;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddRazorPages();
//builder.Services.AddDistributedMemoryCache();
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromMinutes(30);
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//});
//builder.Services.AddSignalR();

//// 1. BẮT BUỘC: Phải có cái này thì Handler mới lấy được Session
//builder.Services.AddHttpContextAccessor();

//// 2. Đăng ký cái Handler mình vừa viết
//builder.Services.AddTransient<AuthHeaderHandler>();
//builder.Services.AddTransient<OfflineHandler>();
//builder.Services.AddHostedService<CacheRefreshService>();



//var circuitBreakerPolicy = HttpPolicyExtensions
//    .HandleTransientHttpError()
//    .CircuitBreakerAsync(
//        handledEventsAllowedBeforeBreaking: 5,
//        durationOfBreak: TimeSpan.FromSeconds(30)
//    );

//var retryPolicy = HttpPolicyExtensions
//    .HandleTransientHttpError()
//    .Or<TimeoutRejectedException>()
//    .WaitAndRetryAsync(
//        retryCount: 2,
//        sleepDurationProvider: _ => TimeSpan.FromMilliseconds(100),
//        onRetry: (outcome, timespan, retryCount, context) =>
//        {
//            Console.WriteLine($"[Polly] Fast Retry attempt {retryCount} due to: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
//        }
//    );


//builder.Services.AddHttpClient("NewsAPI", client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7066/");
//})
//.AddHttpMessageHandler<AuthHeaderHandler>()
//.AddHttpMessageHandler<OfflineHandler>()
//.AddPolicyHandler(retryPolicy)
//.AddPolicyHandler(circuitBreakerPolicy);

//builder.Services.AddHttpClient("AnalyticsAPI", client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7078/");
//})
//.AddHttpMessageHandler<AuthHeaderHandler>()
//.AddHttpMessageHandler<OfflineHandler>()
//.AddPolicyHandler(retryPolicy)
//.AddPolicyHandler(circuitBreakerPolicy);

//builder.Services.AddHttpClient("AIAPI", client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7150/");
//})
//.AddHttpMessageHandler<AuthHeaderHandler>()
//.AddPolicyHandler(retryPolicy)
//.AddPolicyHandler(circuitBreakerPolicy);

//builder.Services.AddHttpClient("CoreWorkerClient", client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7066/");
//    client.DefaultRequestHeaders.Add("Accept", "application/json");
//})
//.AddPolicyHandler(retryPolicy)
//.AddPolicyHandler(circuitBreakerPolicy);

//builder.Services.AddHttpClient("AnalyticsWorkerClient", client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7078/");
//    client.DefaultRequestHeaders.Add("Accept", "application/json");
//})
//.AddPolicyHandler(retryPolicy)
//.AddPolicyHandler(circuitBreakerPolicy);

//builder.Services.AddMemoryCache();

//var app = builder.Build();


//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}
//else
//{
//    app.UseExceptionHandler("/Error");
//    app.UseHsts();
//}

//app.UseStatusCodePagesWithReExecute("/Error/{0}");

//app.UseHttpsRedirection();

//app.UseStaticFiles();

//app.UseRouting();

//app.UseSession();

//app.UseAuthorization();

//app.MapRazorPages();
//app.MapHub<ReportHub>("/reportHub");

//app.Run();


