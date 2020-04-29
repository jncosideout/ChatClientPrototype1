using System;
using System.IO;
using System.Runtime.Serialization;

namespace ChatServerPrototype1.Utilities
{
    [DataContract(Name = "MessageContract", Namespace = "ChatServerPrototype1.Utilities")]
    public class Message
    {
        [DataMember]
        public string TheMessage { get; set; }

        public Message(string aMessage)
        {
            TheMessage = aMessage;
        }
    }
}
