using DoQuangThang_SE1885_A01_FE.Helper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DoQuangThang_SE1885_A01_FE.Services
{
    public class CacheRefreshService : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CacheRefreshService> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly TimeSpan _period = TimeSpan.FromHours(6); 

       
        private readonly List<string> _coreEndpoints = new List<string>
        {
            "/api/category",
            "/api/news" 
        };

        private readonly List<string> _analyticsEndpoints = new List<string>
        {
            "/api/analytics/dashboard",
            "/api/analytics/trending"
        };

        public CacheRefreshService(IHttpClientFactory httpClientFactory, ILogger<CacheRefreshService> logger, IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(_period);

            // Chạy ngay lần đầu
            await RefreshAllCachesAsync();

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RefreshAllCachesAsync();
            }
        }

        private async Task RefreshAllCachesAsync()
        {
            _logger.LogInformation($"[Worker] Starting refresh at: {DateTime.Now}");

            // 1. Xử lý Core API
            await FetchAndCacheAsync("CoreWorkerClient", _coreEndpoints);

            // 2. Xử lý Analytics API
            await FetchAndCacheAsync("AnalyticsWorkerClient", _analyticsEndpoints);
        }

        // Hàm chung để xử lý việc gọi và lưu cache
        private async Task FetchAndCacheAsync(string clientName, List<string> endpoints)
        {
            try
            {
                // Lấy đúng Client dựa trên tên đã đăng ký ở Program.cs
                var client = _httpClientFactory.CreateClient(clientName);

                // Lấy BaseAddress để ghép thành URL đầy đủ (làm Key cho Cache)
                string baseAddress = client.BaseAddress?.ToString().TrimEnd('/');

                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        var response = await client.GetAsync(endpoint);

                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsByteArrayAsync();

                            // TẠO KEY CACHE: Phải ghép BaseAddress + Endpoint
                            // Ví dụ: https://localhost:5000/api/news
                            // Điều này cực kỳ quan trọng để OfflineHandler (FE) tìm thấy file này.
                            string fullUrl = $"{baseAddress}/{endpoint.TrimStart('/')}";

                            await LocalStorageHelper.SaveDataAsync(fullUrl, content);

                            _logger.LogInformation($"[Worker] Cached updated for: {fullUrl}");


                            var jsonString = System.Text.Encoding.UTF8.GetString(content);

                            // Lưu vào RAM với Key chính là endpoint (VD: "/api/news")
                            _memoryCache.Set(endpoint, jsonString, _period);

                            _logger.LogInformation($"[Worker] Cached updated for: {fullUrl} (File & RAM)");

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[Worker] Failed endpoint: {endpoint} using {clientName}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Worker] Error connecting with client: {clientName}");
            }
        }
    }
}