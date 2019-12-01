using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ERC.Chat.Engine
{
    public class EncryptedChat 
    {
        public byte[] PublicKey { get; }
        public byte[] PrivateKey { get; private set; }

        public virtual byte[] EncryptionKey => PrivateKey;

        private ECDiffieHellmanCng Crypt { get; }

        public string ChatMember { get; }

        public EncryptedChat(string chatMember)
        {
            Crypt = new ECDiffieHellmanCng
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.Sha256
            };
            PublicKey = Crypt.PublicKey.ToByteArray();
            ChatMember = chatMember;
        }

        public void Bind(Handshake handshake)
        {
            var import = CngKey.Import(handshake.PublicKey, CngKeyBlobFormat.EccPublicBlob);
            PrivateKey = Crypt.DeriveKeyMaterial(import);
        }

        public Handshake CreateHandshake()
        {
            return new Handshake(ChatMember, PublicKey);
        }

        public ChatMessage Encrypt(string message)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = EncryptionKey;

                // Encrypt the chatMessage
                using (var ciphertext = new MemoryStream())
                using (var cs = new CryptoStream(ciphertext, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    var plaintextMessage = Encoding.UTF8.GetBytes(message);
                    cs.Write(plaintextMessage, 0, plaintextMessage.Length);
                    cs.FlushFinalBlock();
                    return new ChatMessage(ChatMember, ciphertext.ToArray(), aes.IV);
                }
            }
        }

        public ChatMessage EncryptData(byte[] data)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = PrivateKey;

                // Encrypt the chatMessage
                using (var ciphertext = new MemoryStream())
                using (var cs = new CryptoStream(ciphertext, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    return new ChatMessage(ChatMember, ciphertext.ToArray(), aes.IV);
                }
            }
        }

        public ChatMessage Decrypt(ChatMessage chatMessage)
        {
            if (!chatMessage.IsEncrypted)
                throw new ArgumentOutOfRangeException(nameof(chatMessage), "The chatMessage is not encrypted.");

            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = EncryptionKey;
                aes.IV = chatMessage.IV;

                ChatMessage message = null;
                try
                {
                    using (var plaintext = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(plaintext, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            var encryptedMessage = chatMessage.EncryptedMessage;
                            cs.Write(encryptedMessage, 0, encryptedMessage.Length);
                            cs.FlushFinalBlock();
                            message = new ChatMessage(chatMessage.Sender, Encoding.UTF8.GetString(plaintext.ToArray()));
                        }
                    }
                }
                catch (Exception exc)
                {
                    // This is mostly because of a wrong shared key
                    Debug.WriteLine($"Message can't decrypted: {exc.Message}");
                }
                return message;
            }
        }
        public byte[] DecryptData(ChatMessage chatMessage)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = PrivateKey;
                aes.IV = chatMessage.IV;

                byte[] message = null;
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            var encryptedMessage = chatMessage.EncryptedMessage;
                            cs.Write(encryptedMessage, 0, encryptedMessage.Length);
                            cs.FlushFinalBlock();
                            message = stream.ToArray();
                        }
                    }
                }
                catch (Exception exc)
                {
                    // This is mostly because of a wrong shared key
                    Debug.WriteLine($"Message can't decrypted: {exc.Message}");
                }
                return message;
            }
        }
    }
}
