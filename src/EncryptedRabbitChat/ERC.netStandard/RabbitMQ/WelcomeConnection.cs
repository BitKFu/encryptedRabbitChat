using System;
using ERC.Chat.Engine;

namespace ERC.RabbitMQ
{
    public abstract class WelcomeConnection<TServerHandshake, TClientHanshake, TSharedSecret> : RabbitConnection 
        where TClientHanshake : Handshake 
        where TServerHandshake : Handshake
        where TSharedSecret : RabbitSharedSecret
    {
        /// <summary> Name of the client queue </summary>
        protected string ClientQueue { get; }

        /// <summary> Name of the server queue </summary>
        protected string ServerQueue { get; }

        /// <summary>
        /// Gets a flag that defines whether the welcome connection shall be automatically disposed
        /// </summary>
        public bool AutoDispose { get; }

        /// <summary> True, if the welcome connection is in a faulted state - e.g. rejected server invitation</summary>
        public bool FaultedState { get; protected set; }

        /// <summary>
        /// This delegate will be used to process the server handshake reply message
        /// </summary>
        /// <param name="sender">Connection that has send the reply</param>
        /// <param name="chatMember">chat member</param>
        /// <param name="reply">Handshake reply message</param>
        public delegate void SharedKeyExchanged(IDisposable sender, string chatMember, TSharedSecret reply);

        /// <summary>
        /// Creates a welcome connection for the rabbit mq instance.
        /// </summary>
        /// <param name="hostname">Name of the Rabbit MQ Server</param>
        /// <param name="queue">Unique Queue name</param>
        /// <param name="welcomeVirtualHost">Virtual Host that is used for the initial handshake</param>
        /// <param name="welcomeUser">Common known user that can be used for initial handshake</param>
        /// <param name="welcomePassword">Password of the welcome user</param>
        /// <param name="autoDispose">True, if the welcome connection shall be disposed automatically</param>
        public WelcomeConnection(string hostname, string queue, string welcomeVirtualHost, string welcomeUser, string welcomePassword, bool autoDispose = true)
            : base(hostname, welcomeVirtualHost, welcomeUser, welcomePassword)
        {
            ClientQueue = queue + ".Client";
            ServerQueue = queue + ".Server";
            AutoDispose = autoDispose;
        }
    }
}
