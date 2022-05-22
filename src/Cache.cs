using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ThreadMessaging
{

    public class CacheItem<T>
    {
        public T Value { get; set; }
        public DateTime Expiration { get; set; }
    }

    public class Cache<T>
    {
        List<CacheItem<T>> _cache = new List<CacheItem<T>>();

        private void Purge()
        {
            lock (_cache)
            {
                List<CacheItem<T>> toRemove = new List<CacheItem<T>>();
                foreach (CacheItem<T> item in _cache)
                    if (item.Expiration < DateTime.Now)
                        toRemove.Add(item);
                foreach (CacheItem<T> item in toRemove)
                    _cache.Remove(item);
            }
        }

        public void Add(T value, int expirationInSeconds)
        {
            Purge();
            CacheItem<T> item = new CacheItem<T>();
            item.Value = value;
            item.Expiration = DateTime.Now.AddSeconds(expirationInSeconds);
            lock (_cache)
                _cache.Add(item);
        }

        public bool Contains(T value)
        {
            Purge();
            lock (_cache)
                foreach (CacheItem<T> item in _cache)
                    if (item.Value.Equals(value))
                        return true;
            return false;
        }
    }
}
