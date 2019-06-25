using System;
using System.Collections.Generic;
using Reloaded.Mod.Loader.Server.Messages;
using Reloaded.Mod.Loader.Server.Structs;

namespace Reloaded.Mod.Loader.Server
{
    /// <summary>
    /// Provides a generic mechanism for dispatching messages received from a client or server.
    /// Works by assigning functions to specified message "types", declared by <see cref="TMessageType"/>.
    /// </summary>
    /// <typeparam name="TMessageType">Type of value to map to individual message handlers.</typeparam>
    public class MessageHandler<TMessageType>
    {
        private Dictionary<TMessageType, Action<RawNetMessage>> _mapping;

        public MessageHandler()
        {
            _mapping = new Dictionary<TMessageType, Action<RawNetMessage>>();
        }

        /// <summary>
        /// Given a raw network message, decodes the message and delegates it to an appropriate handling method.
        /// </summary>
        public void Handle(RawNetMessage parameters)
        {
            var messageType = MessageBase<TMessageType>.GetMessageType(parameters.Message);
            if (_mapping.TryGetValue(messageType, out Action<RawNetMessage> value))
            {
                value(parameters);
            }
        }

        /// <summary>
        /// Sets a method to execute handling a specific <see cref="TMessageType"/>
        /// </summary>
        public void AddOrOverrideHandler<TStruct>(TMessageType messageType, Action<NetMessage<TStruct>> handler) where TStruct : IMessage<TMessageType>
        {
            Action<RawNetMessage> parameters = rawMessage =>
            {
                var message    = Message<TMessageType, TStruct>.Deserialize(rawMessage.Message);
                var netMessage = new NetMessage<TStruct>(message.ActualMessage, rawMessage);
                handler(netMessage);
            };

            _mapping[messageType] = parameters;
        }

        /// <summary>
        /// Removes the current method assigned to a handle a message of a specific <see cref="TMessageType"/>
        /// </summary>
        public void RemoveHandler(TMessageType messageType)
        {
            _mapping.Remove(messageType);
        }
    }
}
