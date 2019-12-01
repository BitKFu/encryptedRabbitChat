using ERC.RabbitMQ;

namespace Demo.netFramework.Imlementation
{
    public class SimpleClientWelcomeConnection : ClientWelcomeConnection<ServerHandshake, ClientHandshake, DemoSharedSecret>
    {
        public SimpleClientWelcomeConnection(string hostname, string queue) : base(hostname, queue, @"Aula", @"Join", @"aula")
        {
        }

        #region Overrides of ClientWelcomeConnection<ServerHandshake,ClientHandshake>

        protected override bool ValidateServerHandshake(string chatMember, byte[] publicKey, ServerHandshake fromServer, out ClientHandshake clientHandshake)
        {
            // for demo purpose
            if (chatMember == "leia")
            {
                clientHandshake = new ClientHandshake(chatMember, ErrorCodes.REJECTED_I);
                return false;
            }

            clientHandshake = new ClientHandshake(chatMember, publicKey);
            return true;
        }

        #endregion
    }
}
