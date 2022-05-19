using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThreadMessaging
{
    public class MessagingService
    {
        ConcurrentDictionary<string, List<IMessageReceiver>> _subScriptions = new ConcurrentDictionary<string, List<IMessageReceiver>>();

        public ICollection<string> GetGroupList()
        {
            return _subScriptions.Keys;
        }

        public int GetSubscriberCount(string group)
        {
            if (_subScriptions.TryGetValue(group, out List<IMessageReceiver> receivers))
                return receivers.Count;
            return 0;
        }

        public void Subscribe(string group, IMessageReceiver receiver)
        {
            _subScriptions.AddOrUpdate(group, new List<IMessageReceiver>() { receiver }, (key, value) =>
            {
                lock (value)
                    value.Add(receiver);
                return value;
            });
        }

        public void Unsubscribe(string group, IMessageReceiver receiver)
        {
            if (_subScriptions.TryGetValue(group, out List<IMessageReceiver> receivers))
            {
                lock (receivers)
                {
                    receivers.Remove(receiver);
                    if (receivers.Count == 0)
                        _subScriptions.TryRemove(group, out List<IMessageReceiver> receivers2);
                }
            }
        }

        public void Publish(Message message)
        {
            if (_subScriptions.TryGetValue(message.group, out List<IMessageReceiver> receivers))
            {
                List<IMessageReceiver> listClone = null;
                lock (receivers)
                    listClone = new List<IMessageReceiver>(receivers);
                Parallel.ForEach(listClone, async receiver =>
                {
                    await receiver.NewMessageAsync(message);
                });
            }
        }
    }
}