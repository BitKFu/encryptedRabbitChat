using System;

namespace ERC.Chat.Engine
{
    /// <summary>
    /// Basic container for chat messages
    /// </summary>
    [Serializable]
    public class ChatMessage
    {
        /// <summary> Sender of the message </summary>
        public string Sender { get; set; }

        /// <summary> Plain text message, if the message is not encrypted </summary>
        public string PlainText { get; set; }

        public byte[] EncryptedMessage { get; set; }
        public byte[] IV { get; set; }

        public bool IsEncrypted { get; set; }
        
        public ChatMessage()
        {
        }

        public ChatMessage(string sender, string plainText)
        {
            PlainText = plainText;
            IsEncrypted = false;
            Sender = sender;
        }

        public ChatMessage(string sender, byte[] encryptedMessage, byte[] iv)
        {
            EncryptedMessage = encryptedMessage;
            IV = iv;
            IsEncrypted = true;
            Sender = sender;
        }
    }
}
