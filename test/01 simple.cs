using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using ThreadMessaging;

namespace test
{
    [TestClass]
    public class _01Simple : IMessageReceiver
    {
        readonly Message _testMsg = new Message("test", "msg", "hello", "world");
        CountdownEvent _cde = new CountdownEvent(1);
        public Task NewMessageAsync(Message message)
        {
            if (message.group == _testMsg.group
                && message.type == _testMsg.type
                && message.data == _testMsg.data
                && message.context == _testMsg.context)
            {
                _cde.Signal();
            }
            return Task.CompletedTask;
        }

        [TestMethod]
        public void TestMethod1()
        {
            MessagingService service = new MessagingService();
            service.Subscribe(_testMsg.group, this);
            service.Publish(_testMsg);
            Assert.IsTrue(_cde.Wait(1000));
            service.Unsubscribe(_testMsg.group, this);
            Assert.IsTrue(service.GetGroupList().Count == 0);
        }
    }
};