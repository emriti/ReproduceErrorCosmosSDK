using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Nexus.Base.CosmosDBRepository
{
    public class CacheManager
    {
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string cacheConnection = Environment.GetEnvironmentVariable("CacheConnection");
            return ConnectionMultiplexer.Connect(cacheConnection);
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        // todo: cek lagi apakah GetDatabase seharusnya disimpan sebagai variable agar tidak connect berulang-ulang
        public static IDatabase Database
        {
            get
            {
                return lazyConnection.Value.GetDatabase();
            }
        }

        public static async Task<T> GetObject<T>(string key)
        {
            var value = await Database.StringGetAsync(key);
            if (value == RedisValue.Null) return default(T);

            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public static async Task<bool> SetObject(string key, object value, TimeSpan? expiry = null)
        {
            if (expiry == null)
            {
                expiry = DateTime.UtcNow.AddDays(1).TimeOfDay;
            }

            return await Database.StringSetAsync(key, JsonConvert.SerializeObject(value), expiry);
        }

        public static async Task<bool> RemoveObject(string key)
        {
            return await Database.KeyDeleteAsync(key);
        }

    }
}
