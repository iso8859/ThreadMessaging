using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadMessaging
{
    public class Message
    {
        public Message()
        {

        }

        public Message(string _group, string _type, string _data = null, object _context = null)
        {
            group = _group;
            type = _type;
            data = _data;
            context = _context;
        }
        public string group { get; set; }   // This is the group this message has been sent to.
        public string type { get; set; }    // Message type, for example "refresh" or "update"
        public string data { get; set; }    // data, for example a JSON string
        public object context { get; set; } // dev context
    }

    public interface IMessageReceiver
    {
        Task NewMessageAsync(Message message);
    }

}
