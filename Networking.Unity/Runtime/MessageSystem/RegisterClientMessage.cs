using System;
using System.Linq;
using System.Reflection;
using Debugging.DeveloperConsole;
using Installation01.Networking.NetStack.Serialization;
using UnityEngine;

namespace Installation01.Networking
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterClientMessage : Attribute
    {
        private readonly ushort id;

        internal RegisterClientMessage(BuiltInMessage id)
        {
            this.id = (ushort)id;
        }
        
        public RegisterClientMessage(ushort id)
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
						
                        var command = method.GetCustomAttribute<RegisterClientMessage>();

                        // var parameters = method.GetParameters();
                        // var paramList = parameters.Select(parameter => parameter.ParameterType).ToArray();

                        if(command == null)
                            continue;

                        var action = (Action<BitBuffer>) Delegate.CreateDelegate(typeof(Action<BitBuffer>), method);
                        ClientMessageProcessor.TryRegisterCallback(command.id, action);
                    }
                }
            }
        }
    }
}