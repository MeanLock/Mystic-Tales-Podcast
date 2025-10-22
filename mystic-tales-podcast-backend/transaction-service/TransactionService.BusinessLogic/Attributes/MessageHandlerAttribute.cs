using System;

namespace TransactionService.BusinessLogic.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MessageHandlerAttribute : Attribute
    {
        public string MessageType { get; }
        public string Topic { get; }

        public MessageHandlerAttribute(string messageType, string topic)
        {
            MessageType = messageType;
            Topic = topic;
        }
    }
}
