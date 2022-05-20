using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadMessaging
{
    public class Message
    {
        public string Uid { get; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; } = DateTime.Now;
        public Message()
        {

        }

        public Message(string _group, string _type, string _data = null)
        {
            group = _group;
            type = _type;
            data = _data;
        }
        public string group { get; set; }   // This is the group this message has been sent to.
        public string type { get; set; }    // Message type, for example "refresh" or "update"
        public string data { get; set; }    // data, for example a JSON string
    }    

    public abstract class MessageReceiver
    {
        public Cache<string> _cache = new Cache<string>();
        public abstract Task NewMessageAsync(Message message);
    }

}
