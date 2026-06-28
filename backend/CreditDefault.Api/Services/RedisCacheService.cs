using System.Text.Json;
using CreditDefault.Api.Interfaces;
using StackExchange.Redis;

namespace CreditDefault.Api.Services
{
    public class RedisCacheService : ICacheService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly IConnectionMultiplexer? _redis;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly string _instanceName;

        public RedisCacheService(IConfiguration configuration, ILogger<RedisCacheService> logger)
        {
            _logger = logger;
            _instanceName = Environment.GetEnvironmentVariable("REDIS_INSTANCE_NAME")
                ?? configuration["Redis:InstanceName"]
                ?? "CreditDefault";
            _redis = TryConnect(configuration);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var database = GetDatabase();
                if (database == null) return default;

                var value = await database.StringGetAsync(FormatKey(key));
                if (value.IsNullOrEmpty) return default;

                return JsonSerializer.Deserialize<T>(value.ToString(), JsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis get failed for key {CacheKey}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var database = GetDatabase();
                if (database == null) return;

                var payload = JsonSerializer.Serialize(value, JsonOptions);
                await database.StringSetAsync(FormatKey(key), payload, expiration ?? TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis set failed for key {CacheKey}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                var database = GetDatabase();
                if (database == null) return;

                await database.KeyDeleteAsync(FormatKey(key));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis remove failed for key {CacheKey}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var database = GetDatabase();
                return database != null && await database.KeyExistsAsync(FormatKey(key));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis exists failed for key {CacheKey}", key);
                return false;
            }
        }

        private IDatabase? GetDatabase()
        {
            if (_redis == null || !_redis.IsConnected) return null;
            return _redis.GetDatabase();
        }

        private RedisKey FormatKey(string key) => $"{_instanceName}:{key}";

        private IConnectionMultiplexer? TryConnect(IConfiguration configuration)
        {
            var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
                ?? configuration["Redis:ConnectionString"];

            if (string.IsNullOrWhiteSpace(redisConnectionString))
            {
                _logger.LogInformation("Redis is not configured. SQL fallback will be used.");
                return null;
            }

            try
            {
                var options = ConfigurationOptions.Parse(redisConnectionString);
                options.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis connection failed. SQL fallback will be used.");
                return null;
            }
        }
    }
}
