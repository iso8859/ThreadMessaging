using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadMessaging
{
    public class Tenant : ConcurentDicList<MessageReceiver>
    {
        public string tenantId { get; }
        private MessagingService msgSvc;
        public Tenant(MessagingService msgSvc, string tenantId)
        {
            this.tenantId = tenantId;
            this.msgSvc = msgSvc;
        }
        public Task SubscribeAsync(string groupId, MessageReceiver receiver) => AddAsync(groupId, receiver);
        public override Task OnAddedAsync(string key, bool newKey, MessageReceiver obj) => msgSvc.OnSubscribeAsync != null ? msgSvc.OnSubscribeAsync(this, key, newKey, obj) : Task.CompletedTask;
        public Task UnsubscribeAsync(string groupId, MessageReceiver receiver) => RemoveAsync(groupId, receiver);
        public override Task OnRemovedAsync(string key, bool groupDeleted, MessageReceiver obj) => msgSvc.OnUnsubscribeAsync != null ? msgSvc.OnUnsubscribeAsync(this, key, groupDeleted, obj) : Task.CompletedTask;
        public Task PublishAsync(Message message, int expireInSecond = 60, CancellationToken cancel = default)
        {
            if (message.tenantId != tenantId)
                throw new Exception("TenantId mismatch. Use MessagingService.PublishAsync instead.");
            if (TryGetValue(message.groupId, out List<MessageReceiver> receivers))
            {
                List<MessageReceiver> listClone = null;
                lock (receivers)
                    listClone = new List<MessageReceiver>(receivers);
                Parallel.ForEach(listClone, async receiver =>
                {
                    if (!receiver._cache.Contains(message.Uid))
                    {
                        receiver._cache.Add(message.Uid, expireInSecond);
                        await receiver.NewMessageAsync(message, cancel);
                    }
                });
            }
            return Task.CompletedTask;
        }
        public Task PublishExceptAsync(Message message, int expireSecond, params MessageReceiver[] msgReceivers)
        {
            if (message.tenantId != tenantId)
                throw new Exception("TenantId mismatch. Use MessagingService.PublishAsync instead.");
            if (msgReceivers != null)
            {
                foreach (MessageReceiver msgreceiver in msgReceivers)
                {
                    msgreceiver._cache.Add(message.Uid, expireSecond);
                }
            }
            return PublishAsync(message, expireSecond);
        }
        public Message NewMessage(string groupId, string message, string data = null) => new Message(tenantId, groupId, message, data);
        public Message NewMessageFromTemplate(Message msgTemplate, string data) => new Message(tenantId, msgTemplate.groupId, msgTemplate.type, data);
        public bool MessageMatch(Message message, string groupId) => message.tenantId == tenantId && message.groupId == groupId && message.type == message.type;
        public bool MessageMatchTemplate(Message message, Message msgTemplate) => message.tenantId == tenantId && message.groupId == msgTemplate.groupId && message.type == msgTemplate.type;
    }

    public class MessagingService
    {
        ConcurrentDictionary<string, Tenant> _tenants = new ConcurrentDictionary<string, Tenant>();
        public Tenant OpenTenant(string tenantId) => _tenants.AddOrUpdate(tenantId, new Tenant(this, tenantId), (key, value) => value);
        public Tenant OpenTenant(string tenantId, Tenant newTenant) => _tenants.AddOrUpdate(tenantId, newTenant, (key, value) => newTenant);
        public Func<Tenant, string, bool, MessageReceiver, Task> OnSubscribeAsync; // Tenant, groupId, newgroup, MessageReceiver
        public Func<Tenant, string, bool, MessageReceiver, Task> OnUnsubscribeAsync; // Tenant, groupId, groupDeleted, MessageReceiver, 
        public async Task PublishAsync(Message message, int expireInSecond = 60, CancellationToken cancellation = default)
        {
            if (_tenants.TryGetValue(message.tenantId, out Tenant tenant))
                await tenant.PublishAsync(message, expireInSecond);
        }

        public List<string> GetTenantList() => _tenants.Keys.ToList();
    }
}