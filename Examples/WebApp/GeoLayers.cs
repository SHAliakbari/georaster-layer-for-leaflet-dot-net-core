using georaster_layer_for_leaflet_dot_net_core;
using Microsoft.Extensions.Caching.Memory;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WebApp
{
    public class GeoLayers : BackgroundService
    {
        //private static ConcurrentDictionary<string, geoLayer> pairs = new ConcurrentDictionary<string, geoLayer>();
        private static ConcurrentDictionary<string, object> locks = new ConcurrentDictionary<string, object>();
        private static object _lock = new object();
        private readonly IMemoryCache memoryCache;
        private readonly ILogger<GeoLayers> logger;
        public static List<string> keys = new List<string>();

        public GeoLayers(IMemoryCache memoryCache, ILogger<GeoLayers> logger)
        {
            this.memoryCache=memoryCache;
            this.logger=logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                RemoveExpiredItems();
                await Task.Delay(TimeSpan.FromMinutes(12), stoppingToken); // Adjust the interval as needed
            }
        }

        private void RemoveExpiredItems()
        {
            // Iterate through the cache and remove expired items
            // Dispose the items as needed
            var keysToRemove = new List<object>();
            foreach (var key in keys)
            {
                geoLayer geoLayer;
                if (!memoryCache.TryGetValue(key, out geoLayer))
                {
                    keysToRemove.Add(key);
                }
            }

            keys.RemoveAll(keysToRemove.Contains);
            object tmp;
            foreach (var key in keysToRemove)
                locks.TryRemove(key.ToString(), out tmp);
        }

        public geoLayer Get(string key, Func<geoLayer> createCallback)
        {
            geoLayer geoLayer;
            if (!memoryCache.TryGetValue(key, out geoLayer))
            {
                ///grabbing the lock for this key
                var keyLock = locks.GetValueOrDefault(key);
                if (keyLock==null)
                {
                    lock (_lock)
                    {
                        keyLock = locks.GetValueOrDefault(key);
                        if (keyLock==null)
                        {
                            keyLock = new object();
                            locks.TryAdd(key, keyLock);
                        }
                    }
                }

                lock (keyLock)
                {
                    if (!memoryCache.TryGetValue(key, out geoLayer))
                    {
                        Console.WriteLine($"*****************{key}");
                        geoLayer = createCallback();

                        var option = new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(10),

                        };

                        option.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration()
                        {
                            EvictionCallback =
                             (object key, object? value, EvictionReason reason, object? state) =>
                             {
                                 if (value is geoLayer)
                                 {
                                     logger.LogDebug($"Cleaning {key}");
                                     (value as geoLayer).Dispose();
                                     value = null;
                                 }
                             }
                        });

                        memoryCache.Set(key, geoLayer, option);
                        keys.Add(key);
                        //pairs.TryAdd(key, createCallback());
                    }
                }
            }
            //memoryCache.TryGetValue(key, out geoLayer);
            return geoLayer;

        }

        public static int CalculateSize(object obj)
        {
            if (obj == null)
                return 0;

            Hashtable visited = new Hashtable();
            return CalculateSizeInternal(obj, visited);
        }

        private static int CalculateSizeInternal(object obj, Hashtable visited)
        {
            if (obj == null || visited.ContainsKey(obj))
                return 0;

            visited.Add(obj, null);
            int size = 0;

            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                try
                {


                    object fieldValue = field.GetValue(obj);
                    if (fieldValue == null)
                        continue;

                    Type fieldType = fieldValue.GetType();

                    if (fieldType.IsValueType || fieldType == typeof(string))
                    {
                        size += Marshal.SizeOf(fieldType);
                    }
                    else if (fieldType.IsClass)
                    {
                        size += CalculateSizeInternal(fieldValue, visited);
                    }
                }
                catch
                {

                }
            }

            return size;
        }

        //public geoLayer GetGeoLayer(string key)
        //{
        //    geoLayer res;
        //    if(pairs.TryGetValue(key, out res)) return res;
        //    return null;
        //}



    }
}
