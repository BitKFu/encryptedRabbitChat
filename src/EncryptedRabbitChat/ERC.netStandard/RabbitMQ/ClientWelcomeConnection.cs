using System;
using ERC.Chat.Engine;
using RabbitMQ.Client.Events;

namespace ERC.RabbitMQ
{
    public abstract class ClientWelcomeConnection<TServerHandshake, TClientHandshake, TSharedSecret> : WelcomeConnection<TServerHandshake, TClientHandshake, TSharedSecret>
        where TClientHandshake : Handshake
        where TServerHandshake : Handshake
        where TSharedSecret : RabbitSharedSecret
    {
        /// <summary> Chat </summary>
        private EncryptedChat Chat { get; set; }

        /// <summary> This event will be send, when the handshake reply will be received from the server, or the server send the shared secret to the client</summary>
        public event SharedKeyExchanged OnSharedKeyExchanged;

        /// <summary>
        /// Creates a welcome connection for the rabbit mq instance.
        /// </summary>
        /// <param name="hostname">Name of the Rabbit MQ Server</param>
        /// <param name="queue">Unique Queue name</param>
        /// <param name="welcomeVirtualHost">Virtual Host that is used for the initial handshake</param>
        /// <param name="welcomeUser">Common known user that can be used for initial handshake</param>
        /// <param name="welcomePassword">Password of the welcome user</param>
        /// <param name="autoDispose">True, if the welcome connection shall be disposed automatically</param>
        public ClientWelcomeConnection(
            string hostname, 
            string queue, 
            string welcomeVirtualHost, 
            string welcomeUser, 
            string welcomePassword, 
            bool autoDispose = true)
            : base(hostname, queue, welcomeVirtualHost, welcomeUser, welcomePassword, autoDispose)
        {
        }

        /// <summary>
        /// Initializes the handshake and sends the public server key to the client
        /// </summary>
        /// <param name="chatMember">Name of the client</param>
        public void InitiateHandshake(string chatMember)
        {
            GetServerPublicKeyAndInitiateHandshake(chatMember);
        }

        /// <summary>
        /// Waits until the client connects to the server
        /// </summary>
        public void WaitForHandshake()
        {
            if (!FaultedState)
                WaitForSharedSecret();
            else
            {
                // Dispose automatically when client is faulted
                if (AutoDispose)
                    Dispose();
            }
        }

        /// <summary>
        /// This method retrieves the public key of the server and initiate the handshake back
        /// </summary>
        /// <param name="name">name of the client</param>
        private void GetServerPublicKeyAndInitiateHandshake(string name)
        {
            try
            {
                Model.QueueDeclarePassive(ClientQueue);
            }
            catch { throw new RabbitMQException(RabbitMQException.ExceptionType.NoClientQueue); }

            var data = Model.BasicGet(ClientQueue, true);
            if (data == null)
                throw new RabbitMQException(RabbitMQException.ExceptionType.NoServerHandshakeData);

            var handshake = BinaryFormatter<TServerHandshake>.FromBinary(data.Body);

            Chat = new EncryptedChat(name);
            Chat.Bind(handshake);
            
            SendClientPublicKeyToServer(handshake);
        }

        /// <summary>
        /// This method can be overwritten by a concrete client to check, if the server request is valid
        /// </summary>
        /// <param name="chatMember">Name of the client chat member</param>
        /// <param name="publicKey">Public Key of the client</param>
        /// <param name="fromServer">Handshake from server</param>
        /// <param name="clientHandshake">Client handshake that will be created</param>
        /// <returns></returns>
        protected abstract bool ValidateServerHandshake(string chatMember, byte[] publicKey, TServerHandshake fromServer, out TClientHandshake clientHandshake);

        /// <summary>
        /// This method sends the public key of the client to the server
        /// </summary>
        private void SendClientPublicKeyToServer(TServerHandshake serverHandshake)
        {
            FaultedState = !ValidateServerHandshake(Chat.ChatMember, Chat.PublicKey, serverHandshake, out var handshake);

            try
            {
                Model.QueueDeclarePassive(ServerQueue);
            }
            catch { throw new RabbitMQException(RabbitMQException.ExceptionType.NoServerQueue);}

            Model.BasicPublish(string.Empty, ServerQueue, true, null, BinaryFormatter<TClientHandshake>.ToBinary(handshake));
        }

        /// <summary>
        /// This method waits until the server send the shared secret to the client
        /// </summary>
        private void WaitForSharedSecret()
        {
            try
            {
                Model.QueueDeclarePassive(ClientQueue);
            }
            catch { throw new RabbitMQException(RabbitMQException.ExceptionType.NoClientQueue); }

            var consumer = new EventingBasicConsumer(Model);
            consumer.Received += (obj, ea) =>
            {
                var message = BinaryFormatter<ChatMessage>.FromBinary(ea.Body);
                var handshake = BinaryFormatter<TSharedSecret>.FromBinary(Chat.DecryptData(message));

                // If we can't decrypt it, leave and go on
                if (handshake == null)
                    return;

                OnSharedKeyExchanged?.Invoke(this, Chat.ChatMember, handshake);

                // Dispose automatically on handshake established
                if (AutoDispose)
                    Dispose();
            };
            Model.BasicConsume(ClientQueue, true, string.Empty, false, true, null, consumer);
        }

        #region Overrides of RabbitConnection

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            base.Dispose();
        }

        #endregion
    }
}
