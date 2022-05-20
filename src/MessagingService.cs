using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThreadMessaging
{
    public class MessagingService
    {
        ConcurrentDictionary<string, List<MessageReceiver>> _subScriptions = new ConcurrentDictionary<string, List<MessageReceiver>>();

        public ICollection<string> GetGroupList()
        {
            return _subScriptions.Keys;
        }

        public int GetSubscriberCount(string group)
        {
            if (_subScriptions.TryGetValue(group, out List<MessageReceiver> receivers))
                return receivers.Count;
            return 0;
        }

        public async Task SubscribeAsync(string group, MessageReceiver receiver)
        {
            bool newGroup = !_subScriptions.ContainsKey(group);
            _subScriptions.AddOrUpdate(group, new List<MessageReceiver>() { receiver }, (key, value) =>
            {
                lock (value)
                    value.Add(receiver);
                return value;
            });
            await OnSubscribeAsync(group, receiver, newGroup);
        }

        public virtual Task OnSubscribeAsync(string group, MessageReceiver receiver, bool newGroup)
        {
            return Task.CompletedTask;
        }

        public async Task UnsubscribeAsync(string group, MessageReceiver receiver)
        {
            if (_subScriptions.TryGetValue(group, out List<MessageReceiver> receivers))
            {
                lock (receivers)
                {
                    receivers.Remove(receiver);
                    if (receivers.Count == 0)
                        _subScriptions.TryRemove(group, out List<MessageReceiver> receivers2);
                }
            }
            await OnUnsubscribeAsync(group, receiver, !_subScriptions.ContainsKey(group));
        }

        public virtual Task OnUnsubscribeAsync(string group, MessageReceiver receiver, bool deletedGroup)
        {
            return Task.CompletedTask;
        }

        public Task PublishAsync(Message message, int expireInSecond=60)
        {
            if (_subScriptions.TryGetValue(message.group, out List<MessageReceiver> receivers))
            {
                List<MessageReceiver> listClone = null;
                lock (receivers)
                    listClone = new List<MessageReceiver>(receivers);
                Parallel.ForEach(listClone, async receiver =>
                {
                    if (!receiver._cache.Contains(message.Uid))
                    {
                        receiver._cache.Add(message.Uid, expireInSecond);
                        await receiver.NewMessageAsync(message);
                    }
                });
            }
            return Task.CompletedTask;
        }

        public Task PublishExceptAsync(Message message, int expireSecond, params MessageReceiver[] msgReceivers)
        {
            if (msgReceivers!=null)
            {
                foreach (MessageReceiver msgreceiver in msgReceivers)
                {
                    msgreceiver._cache.Add(message.Uid, expireSecond);
                }
            }
            return PublishAsync(message, expireSecond);
        }
    }
}