using Microsoft.Extensions.Caching.Memory;
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

        public static void WriteToCache(string key, List<Track> value)
        {
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
                    .SetPriority(CacheItemPriority.Normal);

            try
            {
                _cache.Set(key, value, cacheEntryOptions);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message);
            }
        }

        public static List<Track>? ReadFromCache(string key)
        {
            try
            {
                var cacheResult = _cache.TryGetValue(key, out List<Track>? tracks);

                return tracks;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message);
                return null;
            }
        }
    }
}
