using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Demo.netFramework.Imlementation;
using Demo.netFramework.Storage;
using ERC.RabbitMQ;

namespace Demo.netFramework
{
    class Program
    {
        private static readonly List<GroupChatConnection<DemoSharedSecret, MessageObject>> Chats = new List<GroupChatConnection<DemoSharedSecret, MessageObject>>();

        static void Main(string[] args)
        {
            var storage = new InMemoryChatStorage<DemoSharedSecret>();
            var secret = new DemoSharedSecret("KundeA", "KundeA_User", "4711Kunde", "In love");

            CreateGroupChat("alice", secret);
            InviteUser("bob", secret, storage);
            InviteUser("oliver", secret, storage);
            InviteUser("oliver", secret, storage);
            InviteUser("leia", secret, storage);

            Thread.Sleep(TimeSpan.FromSeconds(5));

            Chats.Reverse();
            foreach (var chat in Chats)
            {
                chat.Dispose();
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            Console.WriteLine("Press return to exit");
            Console.ReadLine();
        }

        private static void InviteUser(
            string userName,
            DemoSharedSecret secret,
            InMemoryChatStorage<DemoSharedSecret> storage)
        {
            var queue = ToASCII(Guid.NewGuid().ToByteArray());

            // Send the public key over the line (the server is initiating this)
            var rabbitServer = new SimpleServerWelcomeConnection("localhost", queue, secret, storage);
            rabbitServer.Connect();

            rabbitServer.InitiateHandshake("alice");
            rabbitServer.WaitForHandshake();

            // Receive the public key from that channel
            var rabbitPhone = new SimpleClientWelcomeConnection("localhost", queue);
            rabbitPhone.OnSharedKeyExchanged += AddUserToGroupChat;
            rabbitPhone.Connect();

            rabbitPhone.InitiateHandshake(userName);
            rabbitPhone.WaitForHandshake();
        }

        private static void CreateGroupChat(string chatMember, DemoSharedSecret sharedSecret)
        {
            var chat = new GroupChatConnection<DemoSharedSecret, MessageObject>("localhost", chatMember, sharedSecret, true);
            chat.OnMessageReceived += WriteMessage;
            chat.OnMetaMessageReceived += WriteMetaMessage;
            chat.Connect();
            chat.SendMessage(new MessageObject("Hello to bob"));

            Chats.Add(chat);
        }

        private static void WriteMetaMessage(string receiver, string sender, MetaMessage message)
        {
            Console.Write("META MESSAGE: ");
            switch (message.State)
            {
                case MetaState.Added:
                    Console.WriteLine($"Added {message.ChatMember}");
                    break;
                case MetaState.Removed:
                    Console.WriteLine($"Removed {message.ChatMember}");
                    break;
                case MetaState.Online:
                    Console.WriteLine($"{message.ChatMember} is online");
                    break;
                case MetaState.Offline:
                    Console.WriteLine($"{message.ChatMember} is offline");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void AddUserToGroupChat(IDisposable sender, string chatMember, DemoSharedSecret sharedSecret)
        {
            if (sharedSecret.ErrorCode == 0)
            {
                var chat = new GroupChatConnection<DemoSharedSecret, MessageObject>("localhost", chatMember, sharedSecret, true);
                chat.OnMessageReceived += WriteMessage;
                chat.Connect();
                chat.SendMessage(new MessageObject("Hello to alice"));

                Chats.Add(chat);
            }
            else
            {
                switch (sharedSecret.ErrorCode)
                {
                    case ErrorCodes.DUPLICATE_I:
                        Console.WriteLine($"Can't add {chatMember}, because its already in the group.");
                        break;
                }
            }
        }

        private static void WriteMessage(string receiver, string sender, MessageObject message)
        {
            Console.WriteLine($"Received by {receiver}:: {sender} says: {message.Message}");
        }

        private static string ToASCII(byte[] rawData)
        {
            // Convert byte array to a string   
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < rawData.Length; i++)
                builder.Append(rawData[i].ToString("x2"));
            return builder.ToString();
        }
    }
}
