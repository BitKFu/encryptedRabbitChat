using System;
using System.Security.Cryptography;

namespace ERC.Chat.Engine
{
    [Serializable]
    public class SharedSecret
    {
        public byte[] SharedKey { get; set; }

        public string GroupChatName { get; set; }

        public SharedSecret()
        {
            var aes = new AesCryptoServiceProvider();
            SharedKey = aes.Key;
        }

        public SharedSecret(string groupChatName)
            :this()
        {
            GroupChatName = groupChatName;
        }

        public SharedSecret(string groupChatName, byte[] sharedKey)
            :this(groupChatName)
        {
            SharedKey = new byte[sharedKey.Length];
            sharedKey.CopyTo(SharedKey, 0);
            GroupChatName = groupChatName;
        }
    }
}
