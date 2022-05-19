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
        readonly Message _testMsg = new Message("test", "msg", "hello", "world");
        public MessagingService service = new MessagingService();
        public volatile int counter;

        [TestMethod]
        public void TestMethod1()
        {
            counter = 10;
            Task[] ts = new Task[10];
            CountdownEvent started = new CountdownEvent(10);
            for (int i=0; i<10; i++)
            {
                int local = i;
                ts[i] = Task.Run(() =>
                {
                    var tmp = new _02Worker(this);
                    tmp.Start(i, started);
                });
            }
            Assert.IsTrue(started.Wait(10000));
            service.Publish(_testMsg);
            Task.WaitAll(ts);
            Assert.IsTrue(counter == 0);
            Assert.IsTrue(service.GetGroupList().Count == 0);
        }
    }

    public class _02Worker : IMessageReceiver
    {
        readonly Message _testMsg = new Message("test", "msg", "hello", "world");
        CountdownEvent _cde = new CountdownEvent(1);
        _02Normal _root;

        public _02Worker(_02Normal root)
        {
            _root = root;
        }

        public Task NewMessageAsync(Message message)
        {
            if (message.group == _testMsg.group
                && message.type == _testMsg.type
                && message.data == _testMsg.data
                && message.context == _testMsg.context)
            {
                Interlocked.Decrement(ref _root.counter);
                _cde.Signal();
            }
            return Task.CompletedTask;
        }

        
        public void Start(int i, CountdownEvent started)
        {
            started.Signal();
            _root.service.Subscribe(_testMsg.group, this);
            bool b = _cde.Wait(1000);
            if (!b)
                Console.WriteLine(i);
            Assert.IsTrue(b);
            _root.service.Unsubscribe(_testMsg.group, this);
        }
    }
};