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

        public async Task SubscribeAsync(string group, IMessageReceiver receiver)
        {
            _subScriptions.AddOrUpdate(group, new List<IMessageReceiver>() { receiver }, (key, value) =>
            {
                lock (value)
                    value.Add(receiver);
                return value;
            });
            await OnSubscribeAsync(group, receiver);
        }

        public virtual Task OnSubscribeAsync(string group, IMessageReceiver receiver)
        {
            return Task.CompletedTask;
        }

        public async Task UnsubscribeAsync(string group, IMessageReceiver receiver)
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
            await OnUnsubscribeAsync(group, receiver);
        }

        public virtual Task OnUnsubscribeAsync(string group, IMessageReceiver receiver)
        {
            return Task.CompletedTask;
        }

        public Task PublishAsync(Message message)
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
            return Task.CompletedTask;
        }
    }
}