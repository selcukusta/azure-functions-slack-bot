using Newtonsoft.Json;
using StackExchange.Redis;
using System;

namespace SlackInteractiveApp
{
    public class RedisCacheProvider
    {
        private static string ClientHost => Environment.GetEnvironmentVariable("REDIS_CONNECTION");

        private static IDatabase Instance
        {
            get
            {
                var redisConnection = ConnectionMultiplexer.Connect(ClientHost);
                return redisConnection.GetDatabase(int.Parse(Environment.GetEnvironmentVariable("REDIS_DEFAULT_DB")));
            }
        }

        public T Get<T>(string key, Func<T> acquire)
        {
            if (Instance.KeyExists(key))
            {
                var redisValue = Instance.StringGet(key);
                if (redisValue.HasValue)
                {
                    return JsonConvert.DeserializeObject<T>(redisValue.ToString());
                }
            }

            var result = acquire();

            if (!object.Equals(result, null))
            {
                Set(key, result);
            }

            return result;
        }

        public void Set<T>(string key, T value)
        {
            if (value == null)
            {
                return;
            }
            var jsonValue = JsonConvert.SerializeObject(value);
            Instance.StringSet(key, jsonValue, TimeSpan.FromDays(7));
        }
    }
}
