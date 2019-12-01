namespace ERC.Chat.Engine
{
    public class EncryptedGroupChat<TSharedSecret> : EncryptedChat
        where TSharedSecret : SharedSecret
    {
        /// <summary>
        /// Creates an encrypted group chat. This constructor is used by the server to create a new group chat with an own secret key
        /// </summary>
        /// <param name="secret">Secret key used for group messages</param>
        /// <param name="chatMember">Name of the chat member that initializes the group chat</param>
        public EncryptedGroupChat(
            TSharedSecret secret, 
            string chatMember)
            : base(chatMember)
        {
            SharedSecret = secret;
        }

        public TSharedSecret SharedSecret { get; private set; }

        public override byte[] EncryptionKey => SharedSecret?.SharedKey ?? PrivateKey;
    }
}
