using System;
using ERC.Chat.Engine;

namespace Demo.netFramework.Imlementation
{
    [Serializable]
    public class ClientHandshake : Handshake
    {
        public int ErrorCode { get; set; }

        public ClientHandshake()
        {
        }

        public ClientHandshake(string chatMember, byte[] publicKey)
            :base(chatMember, publicKey)
        {
        }

        public ClientHandshake(string name, int error)
        {
            Name = name;
            ErrorCode = error;
        }
    }
}
