using System;
using System.Collections.Generic;
using Installation01.Networking.NetStack.Serialization;
using UnityEngine.Assertions;

namespace Installation01.Networking
{
    public class ClientMessageProcessor : IClientMessageHandler
    {
        private static Dictionary<ushort, Action<BitBuffer>> registeredMessages = new Dictionary<ushort, Action<BitBuffer>>();

        public void MessageReceived(BitBuffer data)
        {
            ushort messageId = data.ReadUShort();
            
            Assert.IsNotNull(registeredMessages);
            Assert.IsTrue(registeredMessages.ContainsKey(messageId));
            
            if (!registeredMessages.ContainsKey(messageId))
            {
                return;
            }
            
            registeredMessages[messageId]?.Invoke(data);
        }

        internal static bool TryRegisterCallback(ushort messageId, Action<BitBuffer> callback)
        {
            if (registeredMessages.ContainsKey(messageId))
            {
                return false;
            }
            
            registeredMessages.Add(messageId, callback);
            return true;
        }
        
        internal static bool TryRemoveCallback(ushort messageId)
        {
            Assert.IsNotNull(registeredMessages);
            Assert.IsTrue(registeredMessages.ContainsKey(messageId));
            
            if (!registeredMessages.ContainsKey(messageId))
            {
                return false;
            }
            
            registeredMessages.Remove(messageId);
            return true;
        }
    }
}