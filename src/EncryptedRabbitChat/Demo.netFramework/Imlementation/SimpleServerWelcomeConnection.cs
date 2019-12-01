using System;
using Demo.netFramework.Storage;
using ERC.RabbitMQ;

namespace Demo.netFramework.Imlementation
{
    public class SimpleServerWelcomeConnection : ServerWelcomeConnection<ServerHandshake, ClientHandshake, DemoSharedSecret>
    {
        protected IGroupChatStorage<DemoSharedSecret> GroupChatStorage { get; }

        public SimpleServerWelcomeConnection(string hostname, string queue, DemoSharedSecret secret, IGroupChatStorage<DemoSharedSecret> groupChatStorage) 
            : base(hostname, queue, @"Aula", @"Join", @"aula", secret)
        {
            GroupChatStorage = groupChatStorage;
            if (!GroupChatStorage.GroupChatExists(secret.GroupChatName))
                GroupChatStorage.CreateGroupChat(secret);
        }

        #region Overrides of ServerWelcomeConnection<ServerHandshake,ClientHandshake>

        protected override ServerHandshake CreateServerHandshake(string chatMember, byte[] publicKey)
        {
            if (!GroupChatStorage.IsMemberInGroupChat(Secret.GroupChatName, chatMember))
                GroupChatStorage.AddMember(Secret.GroupChatName, chatMember);

            return new ServerHandshake(chatMember, publicKey);
        }

        #endregion

        protected override bool ValidateClientHandshake(ClientHandshake handshake, out DemoSharedSecret rabbitMqSharedSecret)
        {
            rabbitMqSharedSecret = null;

            switch (handshake.ErrorCode)
            {
                case ErrorCodes.REJECTED_I:
                    Console.WriteLine($"The client {handshake.Name} rejects the invitation.");
                    return false;

                case ErrorCodes.DUPLICATE_I:
                    Console.WriteLine($"The client {handshake.Name} has been invited twice");
                    return false;

                case ErrorCodes.OK_I:
                    // Check, if the member is not already in the group
                    if (!GroupChatStorage.IsMemberInGroupChat(Secret.GroupChatName, handshake.Name))
                    {
                        rabbitMqSharedSecret = Secret;
                        GroupChatStorage.AddMember(Secret.GroupChatName, handshake.Name);
                    }
                    else
                        rabbitMqSharedSecret = new DemoSharedSecret(Secret.GroupChatName, ErrorCodes.DUPLICATE_I);
                    return true;

                default:
                    return false;
            }
        }
    }
}
