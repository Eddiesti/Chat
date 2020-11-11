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
