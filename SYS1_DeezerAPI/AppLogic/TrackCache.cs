using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using SYS1_DeezerAPI.Models;
using SYS1_DeezerAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYS1_DeezerAPI.Services
{
    public static class TrackCache
    {
        private static readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public async static Task<List<Track>> GetOrCreateAsync(string key, Func<ICacheEntry, Task<List<Track>>> factory)
        {
            try
            {
                var result = await _cache.GetOrCreateAsync(key, async entry =>
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
                    .SetPriority(CacheItemPriority.Normal);

                    entry.SetOptions(cacheEntryOptions);

                    LoggerAsync.Log(LogLevel.Trace, $"Writing data to cache. (Query: {key})");
                    return await factory(entry);
                });

                return result ?? [];
            }

            catch (Exception ex)
            {
                LoggerAsync.Log(LogLevel.Error, ex.Message);
                return [];
            }
        }
    }
}
