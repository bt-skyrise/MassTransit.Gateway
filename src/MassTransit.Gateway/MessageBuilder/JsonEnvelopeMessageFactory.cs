using System;
using MassTransit.Gateway.Logging;
using Newtonsoft.Json;

namespace MassTransit.Gateway.MessageBuilder
{
    public static class JsonEnvelopeMessageFactory
    {
        private static readonly ILog Log = LogProvider.GetLogger(typeof(JsonEnvelopeMessageFactory));

        public static MessageEnvelope CreateMessage(string className, string messageJson)
        {
            if (className.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(className));
            if (messageJson.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(messageJson));

            var messageType = MessageTypeCache.TryGetType(className);
            if (messageType == null)
            {
                Log.Warn($"Class {className} was not found in MessageTypeCache. Try to dynamically build type.");
                messageType = DynamicTypeBuilder.BuildMessageType(className, messageJson);
            }

            return new MessageEnvelope(
                JsonConvert.DeserializeObject(messageJson, messageType),
                messageType);
        }

    }
}