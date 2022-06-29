using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadMessaging
{
    // Serializable class
    public class Message
    {
        public string Uid { get; set; } = Guid.NewGuid().ToString(); // get;set; To support serialization
        public DateTime Timestamp { get; set; } = DateTime.Now; // get;set; To support serialization
        public string tenantId { get; set; }    // This is the tenantId 
        public string groupId { get; set; }     // This is the group this message has been sent to.
        public string type { get; set; }        // Message type, for example "refresh" or "update"
        public string data { get; set; }        // data, for example a JSON string
        public string path { get; set; } = "/"; // Path for debug
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

        public override bool Equals(object obj)
        {
            if (obj is Message)
            {
                Message mobj = obj as Message;
                return mobj.tenantId == tenantId && mobj.groupId == groupId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int result = tenantId.GetHashCode();
            result += 31 * result + groupId.GetHashCode();
            result += 31 * result + type.GetHashCode();
            result += 31 * result + data.GetHashCode();
            return result;
        }

        public void AppendPath(string path)
        {
            this.path += path + "/";

        }
    }

    // Receive new messages class.
    // The cache avoid to receive the same message several times.
    public abstract class MessageReceiver
    {
        public Cache<string> _cache = new Cache<string>();
        public abstract Task NewMessageAsync(Message message, CancellationToken cancellation);
    }

    public class GroupBuilder
    {
        Dictionary<string, Message> _messages = new Dictionary<string, Message>();
        string[] _levels;
        string _tenantId;
        public GroupBuilder(string tenantId, params string[] levels)
        {
            _levels = levels;
            _tenantId = tenantId;
        }

        public string Get(int level, string sep = "_")
        {
            return string.Join(sep, _levels.Take(level));
        }

        public Message Get(string name) => _messages[name];
        public Message Add(string name, int level, string type, string sep = "_")
        {
            var m = new Message(_tenantId, Get(level, sep), type);
            _messages[name] = m;
            return m;
        }

        public bool Match(string name, Message msgToCheck)
        {
            Message msg = _messages[name];
            return msg.Equals(msgToCheck) && msg.type == msgToCheck.type;
        }

        public string[] Levels { get => _levels; }
    }
}
