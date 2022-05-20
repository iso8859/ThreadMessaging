using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using ThreadMessaging;

namespace test
{
    [TestClass]
    public class _01Simple : MessageReceiver
    {
        readonly Message _testMsg = new Message("test", "msg", "hello");
        CountdownEvent _cde = new CountdownEvent(1);
        override public Task NewMessageAsync(Message message)
        {
            if (message.group == _testMsg.group
                && message.type == _testMsg.type
                && message.data == _testMsg.data)
            {
                _cde.Signal();
            }
            return Task.CompletedTask;
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            MessagingService service = new MessagingService();
            await service.SubscribeAsync(_testMsg.group, this);
            await service.PublishAsync(_testMsg);
            Assert.IsTrue(_cde.Wait(1000));
            await service.UnsubscribeAsync(_testMsg.group, this);
            Assert.IsTrue(service.GetGroupList().Count == 0);
        }
    }
};