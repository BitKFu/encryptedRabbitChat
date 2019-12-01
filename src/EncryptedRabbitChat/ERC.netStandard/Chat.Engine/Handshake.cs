using System;

namespace ERC.Chat.Engine
{
    [Serializable]
    public class Handshake
    {
        public string Name { get; set; }
        public byte[] PublicKey { get; set; }
        public Handshake()
        {
        }
        
        public Handshake(string name, byte[] publicKey)
        {
            Name = name;
            PublicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
        }
    }
}
