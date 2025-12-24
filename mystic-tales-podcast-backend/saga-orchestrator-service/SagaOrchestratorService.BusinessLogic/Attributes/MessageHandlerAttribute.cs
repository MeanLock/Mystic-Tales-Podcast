using System;

namespace SagaOrchestratorService.BusinessLogic.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MessageHandlerAttribute : Attribute
    {
        public string MessageName { get; }
        public string Topic { get; }

        public MessageHandlerAttribute(string messageName, string topic)
        {
            MessageName = messageName;
            Topic = topic;
        }
    }
}
