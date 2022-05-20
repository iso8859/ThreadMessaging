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
        public volatile int counter;

        [TestMethod]
        public async Task TestMethod1()
        {
            counter = 100;
            
            Task[] ts = new Task[10];
            _03Worker[] workers = new _03Worker[10];
            ManualResetEvent exit = new ManualResetEvent(false);

            CountdownEvent started = new CountdownEvent(10);
            for (int i=0; i<10; i++)
            {
                int local = i;
                ts[i] = Task.Run(async () =>
                {
                    workers[local] = new _03Worker(this);
                    await workers[local].StartAsync(exit, started);
                });
            }
            Assert.IsTrue(started.Wait(1000));
            CountdownEvent cde = new CountdownEvent(100);
            for (int i = 0; i < 10; i++)
                await service.PublishAsync(new Message($"test{i}", "msg", _context: cde));
            Assert.IsTrue(cde.Wait(10000));
            Assert.IsTrue(counter == 0);

            exit.Set();
            Task.WaitAll(ts);
            Assert.IsTrue(service.GetGroupList().Count == 0);
        }
    }

    public class MyMessageReceiver : IMessageReceiver
    {
        public _03Advanced _root;
        public string group;
        public async Task NewMessageAsync(Message message)
        {
            await Task.Delay(250);
            if (message.group == group)
            {
                Interlocked.Decrement(ref _root.counter);
                ((CountdownEvent)message.context).Signal();
            }
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
            started.Signal();
            MyMessageReceiver[] receivers = new MyMessageReceiver[10];
            for (int i = 0; i < 10; i++)
            {
                receivers[i] = new MyMessageReceiver() { _root = _root, group = $"test{i}" };
                await _root.service.SubscribeAsync($"test{i}", receivers[i]);
            }
            bool result = exit.WaitOne(1000);
            for (int i = 0; i < 10; i++)
            {
                await _root.service.UnsubscribeAsync($"test{i}", receivers[i]);
            }
            return result;
        }
    }
};