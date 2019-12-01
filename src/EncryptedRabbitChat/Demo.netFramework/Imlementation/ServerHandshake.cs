using System;
using ERC.Chat.Engine;

namespace Demo.netFramework.Imlementation
{
    [Serializable]
    public class ServerHandshake : Handshake
    {
        public ServerHandshake()
        { }

        public ServerHandshake(string name, byte[] publicKey)
            : base(name, publicKey) 
        { }
    }
}
