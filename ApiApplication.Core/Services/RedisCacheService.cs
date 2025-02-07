using ApiApplication.Core.Abstractions;
using StackExchange.Redis;

namespace ApiApplication.Core.Services
{
    public class RedisCacheService(IConnectionMultiplexer redis) : IRedisCacheService
    {
        private readonly IDatabase database = redis.GetDatabase();

        public async Task SetCacheAsync(string key, string value)
        {
            await database.StringSetAsync(key, value);
        }

        public async Task<string?> GetCacheAsync(string key)
        {
            return await database.StringGetAsync(key);
        }
    }
}