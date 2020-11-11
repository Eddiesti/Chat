using System;
using System.Collections.Generic;
using System.Text;

namespace ChatDesctop.Model
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
