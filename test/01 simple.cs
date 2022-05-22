using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using ThreadMessaging;

namespace test
{
    [TestClass]
    public class _01Simple : MessageReceiver
    {
        Message _msgTemplate;
        CountdownEvent _cde = new CountdownEvent(1);
        Tenant _tenant;
        override public Task NewMessageAsync(Message message)
        {
            if (_tenant.MessageMatchTemplate(message, _msgTemplate) && message.data == "hello")
            {
                _cde.Signal();
            }
            return Task.CompletedTask;
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            MessagingService service = new MessagingService();
            string tenantName = "01simple";
            _tenant = service.OpenTenant(tenantName);
            _msgTemplate = _tenant.NewMessage("test", "msg");
            await _tenant.SubscribeAsync(_msgTemplate.groupId, this);
            await _tenant.PublishAsync(_tenant.NewMessageFromTemplate(_msgTemplate, "hello"));
            Assert.IsTrue(_cde.Wait(1000));
            await _tenant.UnsubscribeAsync(_msgTemplate.groupId, this);
            Assert.IsTrue(_tenant.GetCount(tenantName) == 0);
        }
    }
};