using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using ThreadMessaging;

namespace test
{
    [TestClass]
    public class _02Normal
    {
        public Message _msgTemplate;
        public Tenant _tenant;
        public MessagingService service = new MessagingService();
        public volatile int counter;

        [TestMethod]
        public async Task TestMethod1()
        {
            string tenantName = "02normal";
            _tenant = service.OpenTenant(tenantName);
            _msgTemplate = _tenant.NewMessage("test", "msg");

            counter = 10;
            Task[] ts = new Task[10];
            CountdownEvent started = new CountdownEvent(10);
            for (int i=0; i<10; i++)
            {
                int local = i;
                ts[i] = Task.Run(async () =>
                {
                    var tmp = new _02Worker(this);
                    await tmp.StartAsync(i, started);
                });
            }
            
            Assert.IsTrue(started.Wait(10000));
            await service.PublishAsync(_tenant.NewMessageFromTemplate(_msgTemplate, "hello"));
            Task.WaitAll(ts);
            Assert.IsTrue(counter == 0);
            Assert.IsTrue(_tenant.GetCount(_msgTemplate.groupId) == 0);
        }
    }

    public class _02Worker : MessageReceiver
    {
        CountdownEvent _cde = new CountdownEvent(1);
        _02Normal _root;

        public _02Worker(_02Normal root)
        {
            _root = root;
        }

        override public Task NewMessageAsync(Message message)
        {
            if (_root._tenant.MessageMatchTemplate(message, _root._msgTemplate) && message.data == "hello")
            {
                Interlocked.Decrement(ref _root.counter);
                _cde.Signal();
            }
            return Task.CompletedTask;
        }

        
        public async Task StartAsync(int i, CountdownEvent started)
        {
            await _root._tenant.SubscribeAsync(_root._msgTemplate.groupId, this);
            started.Signal();
            Assert.IsTrue(_cde.Wait(1000));
            await _root._tenant.UnsubscribeAsync(_root._msgTemplate.groupId, this);
        }
    }
};