using System;
using ERC.RabbitMQ;

namespace Demo.netFramework.Imlementation
{
    [Serializable]
    public class DemoSharedSecret : RabbitSharedSecret
    {
        public int ErrorCode { get; set; }

        public DemoSharedSecret()
        {
        }

        public DemoSharedSecret(string groupChatName, int errorCode)
            :base(null, null, null, groupChatName)
        {
            ErrorCode = errorCode;
        }

        public DemoSharedSecret(string virtualHost, string rabbitUser, string rabbitPassword, string groupChatName)
            : base(virtualHost, rabbitUser, rabbitPassword, groupChatName) { }
    }
}
