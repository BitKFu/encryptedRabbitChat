using ERC.Chat.Engine;
using RabbitMQ.Client.Events;

namespace ERC.RabbitMQ
{
    public abstract class ServerWelcomeConnection<TServerHandshake, TClientHandshake, TSharedSecret> : WelcomeConnection<TServerHandshake, TClientHandshake, TSharedSecret>
        where TClientHandshake : Handshake
        where TServerHandshake : Handshake
        where TSharedSecret : RabbitSharedSecret
    {
        protected TSharedSecret Secret { get; }
        private EncryptedGroupChat<TSharedSecret> Chat { get; set; }

        /// <summary> This event will be send, when the handshake reply will be received from the server, or the server send the shared secret to the client</summary>
        public event SharedKeyExchanged OnSharedKeyExchanged;

        /// <summary>
        /// Creates a welcome connection for the rabbit mq instance.
        /// This constructor is used by the server.
        /// </summary>
        /// <param name="hostname">Name of the Rabbit MQ Server</param>
        /// <param name="queue">Unique Queue name</param>
        /// <param name="welcomeVirtualHost">Virtual Host that is used for the initial handshake</param>
        /// <param name="welcomeUser">Common known user that can be used for initial handshake</param>
        /// <param name="welcomePassword">Password of the welcome user</param>
        /// <param name="autoDispose">True, if the welcome connection shall be disposed automatically</param>
        /// <param name="secret">Shared secret</param>
        public ServerWelcomeConnection(
            string hostname, 
            string queue, 
            string welcomeVirtualHost, 
            string welcomeUser, 
            string welcomePassword, 
            TSharedSecret secret, 
            bool autoDispose = true)
            : base(hostname, queue, welcomeVirtualHost, welcomeUser, welcomePassword, autoDispose)
        {
            Secret = secret;
        }

        /// <summary>
        /// Initializes the handshake and sends the public server key to the client
        /// </summary>
        /// <param name="chatMember">Name of the server</param>
        public virtual void InitiateHandshake(string chatMember)
        {
            Chat = new EncryptedGroupChat<TSharedSecret>(Secret, chatMember);
            SendServerPublicKeyToClient();
        }

        /// <summary>
        /// Waits until the client connects to the server
        /// </summary>
        public void WaitForHandshake()
        {
            WaitForClientHandshake();
        }

        /// <summary>
        /// This method can be overwritten by a concrete client to check, if the server request is valid
        /// </summary>
        /// <param name="chatMember">Name of the client chat member</param>
        /// <param name="publicKey">Public Key of the client</param>
        /// <returns></returns>
        protected abstract TServerHandshake CreateServerHandshake(string chatMember, byte[] publicKey);

        /// <summary>
        /// This method sends the public key of the server to the client
        /// </summary>
        private void SendServerPublicKeyToClient()
        {
            var handshake = Chat.CreateHandshake();
            var serverHandshake = CreateServerHandshake(handshake.Name, handshake.PublicKey);

            Model.QueueDeclare(ClientQueue, false, false, true, null);
            Model.BasicPublish(string.Empty, ClientQueue, true, null, BinaryFormatter<TServerHandshake>.ToBinary(serverHandshake));
        }

        /// <summary>
        /// This method waits until the client connects to the server
        /// </summary>
        private void WaitForClientHandshake()
        {
            Model.QueueDeclare(ServerQueue, false, false, true, null);

            var consumer = new EventingBasicConsumer(Model);
            consumer.Received += (obj, ea) =>
            {
                var handshake = BinaryFormatter<TClientHandshake>.FromBinary(ea.Body);
                FaultedState = !ValidateClientHandshake(handshake, out var serverHandshakeReply);
                if (FaultedState)
                {
                    // If the client validation fails, we can dispose the welcome connection
                    if (AutoDispose)
                        Dispose();
                }
                else
                {
                    // Bind server to client
                    Chat.Bind(handshake);

                    // Send it to the client
                    SendHandshakeReply(serverHandshakeReply);
                }
            };
            Model.BasicConsume(ServerQueue, true, string.Empty, false, true, null, consumer);
        }

        protected abstract bool ValidateClientHandshake(TClientHandshake handshake, out TSharedSecret sharedSecret);

        private void SendHandshakeReply(TSharedSecret sharedSecret)
        {
            var message = Chat.EncryptData(BinaryFormatter<TSharedSecret>.ToBinary(sharedSecret));
            var toSend = BinaryFormatter<ChatMessage>.ToBinary(message);

            Model.QueueDeclarePassive(ClientQueue);
            Model.BasicPublish(string.Empty, ClientQueue, true, null, toSend);

            OnSharedKeyExchanged?.Invoke(this, Chat.ChatMember, sharedSecret);

            // Dispose automatically on handshake established
            if (AutoDispose)
                Dispose();
        }

        #region Overrides of RabbitConnection

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            Model.QueueDelete(ServerQueue, false, false);
            base.Dispose();
        }

        #endregion
    }
}
