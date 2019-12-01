using ERC.Chat.Engine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace ERC.RabbitMQ
{
    public class GroupChatConnection<TSharedSecret, TMessageObject> : RabbitConnection
        where TSharedSecret : RabbitSharedSecret
    {
        public delegate void MessageReceived(string receiver, string sender, TMessageObject message);

        public delegate void MetaMessageReceived(string receiver, string sender, MetaMessage message);

        public event MessageReceived OnMessageReceived;
        public event MetaMessageReceived OnMetaMessageReceived;

        protected JSchema MetaMessageSchema { get; }
        protected JSchema ContentMessageSchema { get; }

        private EncryptedGroupChat<TSharedSecret> GroupChat { get; }
        
        public bool FirstAccess { get; private set; }

        public GroupChatConnection(string hostName, string chatMember, TSharedSecret secret, bool sendAddedMetaMessage) 
            : base(hostName, secret.VirtualHost, secret.RabbitUser, secret.RabbitPassword)
        {
            GroupChat = new EncryptedGroupChat<TSharedSecret>(secret, chatMember);

            var generator = new JSchemaGenerator();
            MetaMessageSchema = generator.Generate(typeof(MetaMessage));
            ContentMessageSchema = generator.Generate(typeof(TMessageObject));

            FirstAccess = sendAddedMetaMessage;
        }


        public override void Connect()
        {
            base.Connect();

            // If it's the first access for the user, we send the added meta message
            if (FirstAccess)
                SendMetaMessage(MetaState.Added);

            // Now send a meta message, that the user is online
            SendMetaMessage(MetaState.Online);

            // Declare the queue for the user
            var queue = GroupChat.SharedSecret.GroupChatName + "." + GroupChat.ChatMember;
            Model.QueueDeclare(queue, true, false, false, null);
            var consumer = new EventingBasicConsumer(Model);
            consumer.Received += (obj, ea) =>
            {
                var message = BinaryFormatter<ChatMessage>.FromBinary(ea.Body);

                // Proof that receiver is not the sender
                if (message.Sender == GroupChat.ChatMember)
                    return;

                // Decrypt the message
                if (message.IsEncrypted)
                    message = GroupChat.Decrypt(message);

                // if Message can't decrypted (due to wrong shared key etc)
                if (message == null)
                    return;

                // Check, if the plain text is a json meta object
                var parsedJsonObject = JObject.Parse(message.PlainText);
                if (parsedJsonObject.IsValid(MetaMessageSchema))
                {
                    var metaMessage = JsonConvert.DeserializeObject<MetaMessage>(message.PlainText);
                    OnMetaMessageReceived?.Invoke(GroupChat.ChatMember, message.Sender, metaMessage);
                }
                else if (parsedJsonObject.IsValid(ContentMessageSchema))
                {
                    var contentMessage = JsonConvert.DeserializeObject<TMessageObject>(message.PlainText);
                    OnMessageReceived?.Invoke(GroupChat.ChatMember, message.Sender, contentMessage);
                }
            };

            Model.QueueBind(queue, "amq.fanout", GroupChat.SharedSecret.GroupChatName, null);
            Model.BasicConsume(queue, true, string.Empty, false, true, null, consumer);
        }

        public override void Dispose()
        {
            // Now send a meta message, that the user is offline
            SendMetaMessage(MetaState.Offline);

            base.Dispose();
        }

        protected void SendMetaMessage(MetaState state)
        {
            var metaMessage = new MetaMessage(state, GroupChat.ChatMember);
            var json = JsonConvert.SerializeObject(metaMessage);
            var chatMessage = GroupChat.Encrypt(json);
            Model.BasicPublish("amq.fanout", GroupChat.SharedSecret.GroupChatName, true, new BasicProperties(), BinaryFormatter<ChatMessage>.ToBinary(chatMessage));
        }

        public void SendMessage(TMessageObject message)
        {
            var json = JsonConvert.SerializeObject(message);
            var chatMessage = GroupChat.Encrypt(json);
            Model.BasicPublish("amq.fanout", GroupChat.SharedSecret.GroupChatName, true, new BasicProperties(), BinaryFormatter<ChatMessage>.ToBinary(chatMessage));
        }
    }
}
