using System;

namespace ERC.RabbitMQ
{
    public enum MetaState
    {
        Added,

        Removed,

        Online,

        Offline,
    }
    
    [Serializable]
    public class MetaMessage
    {

        public string ChatMember { get; set; }

        public MetaState State { get; set; }

        public MetaMessage()
        {
        }

        public MetaMessage(MetaState state, string member)
        {
            State = state;
            ChatMember = member;
        }
    }
}
