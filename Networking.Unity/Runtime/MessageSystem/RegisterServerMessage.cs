using System;
using System.Reflection;
using Installation01.Networking.NetStack.Serialization;
using UnityEngine;

namespace Installation01.Networking
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterServerMessage : Attribute
    { 
        private readonly ushort id;

        internal RegisterServerMessage(BuiltInMessage id)
        {
            this.id = (ushort)id;
        }
        
        public RegisterServerMessage(ushort id)
        {
            this.id = id;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void RegisterMessages()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (!method.IsStatic)
                            continue;
						
                        var command = method.GetCustomAttribute<RegisterServerMessage>();

                        // var parameters = method.GetParameters();
                        // var paramList = parameters.Select(parameter => parameter.ParameterType).ToArray();

                        if(command == null)
                            continue;

                        var action = (Action<Peer, BitBuffer>) Delegate.CreateDelegate(typeof(Action<Peer, BitBuffer>), method);
                        ServerMessageProcessor.TryRegisterCallback(command.id, action);
                    }
                }
            }
        }
    }
}