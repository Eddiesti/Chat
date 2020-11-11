using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Model
{
    public class SocketMessage<T>
    {
        public MessageType Type { get; set; }

        public T Payload { get; set; }
    }

    public enum MessageType
    {
        ChatMessage,
        Connect,
        Disconnect
    }
}
