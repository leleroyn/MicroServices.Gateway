using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Newtonsoft.Json;

namespace MicroServices.Gateway.Common
{
    public static class CacheHelper
    {
        private static readonly Cache Cache = HttpRuntime.Cache;
        private static readonly TimeSpan DefaultExpriedTime = new TimeSpan(0, 0, 60);

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">缓存Key值</param>
        /// <returns>缓存的值</returns>
        public static dynamic Get(string key)
        {
            var obj = Cache.Get(key);
            if (obj == null)
            {
                return null;
            }
            return JsonConvert.DeserializeObject(obj.ToString());
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">缓存Key值</param>
        /// <returns>缓存的值</returns>
        public static T Get<T>(string key)
        {
            var obj = Cache.Get(key);
            if (obj == null)
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(obj.ToString());
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key">缓存Key值</param>
        public static void Remove(string key)
        {
            Cache.Remove(key);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存Key值</param>
        /// <param name="value">缓存Value值</param>
        /// <param name="expires">缓存有效时间</param>
        public static void Set(string key, object value, TimeSpan? expires = null)
        {
            expires = expires ?? DefaultExpriedTime;
            value = value ?? string.Empty;
            var content = JsonConvert.SerializeObject(value);

            Cache.Insert(key, content, null, DateTime.Now.AddSeconds(expires.Value.TotalSeconds), Cache.NoSlidingExpiration);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存Key值</param>
        /// <param name="value">缓存Value值</param>
        /// <param name="dependencyFiles">依赖文件</param>
        public static void Set(string key, object value, string[] dependencyFiles = null)
        {
            Cache.Insert(key, value, new CacheDependency(dependencyFiles));
        }
    }
}