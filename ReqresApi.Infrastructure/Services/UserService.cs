
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReqresApi.Application.Interfaces;
using ReqresApi.Domain.Models;
using ReqresApi.Infrastructure.Configuration;

namespace ReqresApi.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<UserService> _logger;
        private readonly ReqresApiOptions _options;

        public UserService(HttpClient httpClient, IMemoryCache cache, IOptions<ReqresApiOptions> options, ILogger<UserService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _cache.GetOrCreateAsync($"user_{id}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                try
                {
                    var response = await _httpClient.GetAsync($"users/{id}");
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return null;

                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    var root = JsonSerializer.Deserialize<JsonElement>(json);
                    return root.GetProperty("data").Deserialize<User>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch user by ID");
                    throw;
                }
            });
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            int page = 1, totalPages;

            do
            {
                try
                {
                    var response = await _httpClient.GetAsync($"users?page={page}");
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();
                    var root = JsonSerializer.Deserialize<JsonElement>(json);

                    totalPages = root.GetProperty("total_pages").GetInt32();
                    var data = root.GetProperty("data").Deserialize<List<User>>();
                    users.AddRange(data ?? new List<User>());
                    page++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error fetching page {page}");
                    throw;
                }
            } while (page <= totalPages);

            return users;
        }
    }
}
