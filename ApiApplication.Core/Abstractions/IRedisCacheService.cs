namespace ApiApplication.Core.Abstractions
{
    public interface IRedisCacheService
    {
        Task SetCacheAsync(string key, string value);
        Task<string?> GetCacheAsync(string key);
    }
}