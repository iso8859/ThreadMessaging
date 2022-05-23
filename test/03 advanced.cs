using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThreadMessaging;

namespace test
{
    [TestClass]
    public class _03Advanced
    {
        public MessagingService service = new MessagingService();
        public Tenant _tenant;
        public Message _msgTemplate;
        public CountdownEvent _cde = new CountdownEvent(100);

        public volatile int counter;

        [TestMethod]
        public async Task TestMethod1()
        {
            counter = 100;

            string tenantName = "03Advanced";
            _tenant = service.OpenTenant(tenantName);
            _msgTemplate = _tenant.NewMessage("test", "msg");

            Task[] ts = new Task[10];
            _03Worker[] workers = new _03Worker[10];
            ManualResetEvent exit = new ManualResetEvent(false);

            CountdownEvent started = new CountdownEvent(10);
            for (int i = 0; i < 10; i++)
            {
                int local = i;
                ts[i] = Task.Run(async () =>
                {
                    workers[local] = new _03Worker(this);
                    await workers[local].StartAsync(exit, started);
                });
            }
            Assert.IsTrue(started.Wait(1000));
            for (int i = 0; i < 10; i++)
            {
                var msg = _tenant.NewMessageFromTemplate(_msgTemplate, i.ToString());
                msg.groupId = $"test{i}";
                await service.PublishAsync(msg);
            }
            Assert.IsTrue(_cde.Wait(10000));
            Assert.IsTrue(counter == 0);

            exit.Set();
            Task.WaitAll(ts);
            Assert.IsTrue(_tenant.GetCount(_msgTemplate.groupId) == 0);
        }
    }

    public class MyMessage : Message
    {
        public object _context;
        public MyMessage(string group, string type, string data = null, object context = null) : base(group, type, data)
        {
            _context = context;
        }
    }
    public class MyMessageReceiver : MessageReceiver
    {
        public _03Advanced _root;
        public string group;

        public override Task NewMessageAsync(Message message, CancellationToken cancel)
        {
            if (message.tenantId == _root._tenant.tenantId && message.groupId == group)
            {
                Interlocked.Decrement(ref _root.counter);
                _root._cde.Signal();
            }
            return Task.CompletedTask;
        }
    }
    public class _03Worker
    {
        _03Advanced _root;
        public _03Worker(_03Advanced root)
        {
            _root = root;
        }

        List<MyMessageReceiver> msgList = new List<MyMessageReceiver>();

        public async Task<bool> StartAsync(ManualResetEvent exit, CountdownEvent started)
        {
            MyMessageReceiver[] receivers = new MyMessageReceiver[10];
            for (int i = 0; i < 10; i++)
            {
                receivers[i] = new MyMessageReceiver() { _root = _root, group = $"test{i}" };
                await _root._tenant.SubscribeAsync($"test{i}", receivers[i]);
            }
            started.Signal();
            bool result = exit.WaitOne(Timeout.Infinite);
            for (int i = 0; i < 10; i++)
            {
                await _root._tenant.UnsubscribeAsync($"test{i}", receivers[i]);
            }
            return result;
        }
    }
};