using System;
using ERC.Chat.Engine;

namespace ERC.RabbitMQ
{
    [Serializable]
    public class RabbitSharedSecret : SharedSecret
    {
        public string VirtualHost { get; set; }
        public string RabbitUser { get; set; }
        public string RabbitPassword { get; set; }

        public RabbitSharedSecret()
        {
        }

        public RabbitSharedSecret(string virtualHost, string rabbitUser, string rabbitPassword, string groupChatName)
            : base(groupChatName)
        {
            VirtualHost = virtualHost;
            RabbitUser = rabbitUser;
            RabbitPassword = rabbitPassword;
        }

        public RabbitSharedSecret(RabbitSharedSecret secret)
             :base(secret.GroupChatName, secret.SharedKey)
        {
            VirtualHost = secret.VirtualHost;
            RabbitUser = secret.RabbitUser;
            RabbitPassword = secret.RabbitPassword;
        }
    }
}
