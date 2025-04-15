
using Microsoft.Extensions.Caching.Memory;

namespace DgLandCrawler.Abstraction.Behaviour
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private static readonly TimeSpan DefaultExpirtion = TimeSpan.FromMinutes(5);
        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<T> GetOrCreate<T>(string key
            , Func<CancellationToken, Task<T>> factory
            , TimeSpan? expiration
            , CancellationToken cancellationToken = default)
        {
            var result =  await _memoryCache.GetOrCreateAsync(key
                ,entry =>
                    {
                        entry.SetAbsoluteExpiration(DefaultExpirtion);

                        return factory(cancellationToken);
                    });

            return result!;
        }
    }
}
