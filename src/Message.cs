using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadMessaging
{
    // Serializable class
    public class Message
    {
        public string Uid { get; set; } = Guid.NewGuid().ToString(); // get;set; To support serialization
        public DateTime Timestamp { get; set; } = DateTime.Now; // get;set; To support serialization
        public string tenantId { get; set; }// This is the tenantId 
        public string groupId { get; set; }   // This is the group this message has been sent to.
        public string type { get; set; }    // Message type, for example "refresh" or "update"
        public string data { get; set; }    // data, for example a JSON string
        public Message()
        {

        }

        public Message(string tenantId, string groupId, string type, string data = null)
        {
            this.tenantId = tenantId;
            this.groupId = groupId;
            this.type = type;
            this.data = data;
        }
        public Message(string groupId, string type)
        {
            this.groupId = groupId;
            this.type = type;
        }

        public override string ToString()
        {
            return tenantId + "_" + groupId;
        }
    }

    // Receive new messages class.
    // The cache avoid to receive the same message several times.
    public abstract class MessageReceiver
    {
        public Cache<string> _cache = new Cache<string>();
        public abstract Task NewMessageAsync(Message message, CancellationToken cancellation);
    }

}
