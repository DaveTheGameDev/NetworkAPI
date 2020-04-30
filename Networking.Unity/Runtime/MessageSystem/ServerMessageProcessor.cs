using System;
using System.Collections.Generic;
using Installation01.Networking.NetStack.Serialization;
using UnityEngine.Assertions;

namespace Installation01.Networking
{
    public class ServerMessageProcessor : IServerMessageHandler
    {
        private static Dictionary<ushort, Action<Peer, BitBuffer>> registeredMessages = new Dictionary<ushort, Action<Peer, BitBuffer>>();

        public void MessageReceived(Peer sender, BitBuffer data)
        {
            ushort messageId = data.ReadUShort();
            
            Assert.IsNotNull(registeredMessages);
            Assert.IsTrue(registeredMessages.ContainsKey(messageId));
            
            if (!registeredMessages.ContainsKey(messageId))
            {
                return;
            }
            
            registeredMessages[messageId]?.Invoke(sender, data);
        }
        
        internal static bool TryRegisterCallback(ushort messageId, Action<Peer, BitBuffer> callback)
        {
            Assert.IsNotNull(registeredMessages);
            Assert.IsTrue(!registeredMessages.ContainsKey(messageId));
            
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