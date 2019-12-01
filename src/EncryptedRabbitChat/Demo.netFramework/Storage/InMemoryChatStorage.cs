using System.Collections.Generic;
using ERC.Chat.Engine;

namespace Demo.netFramework.Storage
{
    public class InMemoryChatStorage<TSharedSecret> : IGroupChatStorage<TSharedSecret>
        where TSharedSecret : SharedSecret
    {
        private Dictionary<string, HashSet<string>> groupChats = new Dictionary<string, HashSet<string>>();
        private Dictionary<string, TSharedSecret> sharedSecrets = new Dictionary<string, TSharedSecret>();

        #region Implementation of IGroupChatStorage

        public bool GroupChatExists(string groupChatName)
        {
            return sharedSecrets.ContainsKey(groupChatName);
        }

        public void CreateGroupChat(TSharedSecret secret)
        {
            sharedSecrets.Add(secret.GroupChatName, secret);
        }

        public TSharedSecret GetSharedSecret(string groupChatName)
        {
            return sharedSecrets[groupChatName];
        }

        public bool IsMemberInGroupChat(string groupChatName, string chatMember)
        {
            if (!groupChats.ContainsKey(groupChatName))
                return false;

            var members = groupChats[groupChatName];
            return members.Contains(chatMember);
        }

        public void AddMember(string groupChatName, string chatMember)
        {
            if (!groupChats.ContainsKey(groupChatName))
                groupChats.Add(groupChatName, new HashSet<string>());

            var members = groupChats[groupChatName];
            members.Add(chatMember);
        }

        #endregion
    }
}
