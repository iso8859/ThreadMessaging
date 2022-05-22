using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ThreadMessaging
{
    public class ConcurentDicList<T> : ConcurrentDictionary<string, List<T>>
    {
        public int GetCount(string key)
        {
            if (TryGetValue(key, out List<T> list))
                return list.Count;
            return 0;
        }

        public async Task AddAsync(string key, T obj)
        {
            bool newKey = true;
            AddOrUpdate(key, new List<T>() { obj }, (k, o) =>
            {
                newKey = false;
                lock (o)
                    o.Add(obj);
                return o;
            });
            await OnAddedAsync(key, newKey, obj);
        }

        public virtual Task OnAddedAsync(string key, bool newKey, T obj) => Task.CompletedTask;

        public async Task RemoveAsync(string key, T obj)
        {
            bool deleted = false;
            if (TryGetValue(key, out List<T> list))
            {
                lock (list)
                {
                    list.Remove(obj);
                    if (list.Count == 0)
                    {
                        deleted = true;
                        TryRemove(key, out List<T> ignore);
                    }
                }
                await OnRemovedAsync(key, deleted, obj);
            }
        }

        public virtual Task OnRemovedAsync(string key, bool keyDeleted, T obj) => Task.CompletedTask;
    }
}
