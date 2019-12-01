using ERC.Chat.Engine;

namespace Demo.netFramework.Storage
{
    public interface IGroupChatStorage<TSharedSecret>
     where TSharedSecret : SharedSecret
    {
        /// <summary>
        /// Returns true, if the group chat already exists
        /// </summary>
        /// <param name="groupChatName">Name of the group chat</param>
        /// <returns>True, if the group chat exists</returns>
        bool GroupChatExists(string groupChatName);

        /// <summary>
        /// Creates a new group chat with the shared secret value
        /// </summary>
        /// <param name="secret">Secret value</param>
        void CreateGroupChat(TSharedSecret secret);

        /// <summary>
        /// Returns the shared secret of the group chat
        /// </summary>
        /// <param name="groupChatName">Name of the group chat</param>s
        /// <returns></returns>
        TSharedSecret GetSharedSecret(string groupChatName);

        /// <summary>
        /// Checks, if a member is already defined within a group chat 
        /// </summary>
        /// <param name="groupChatName">Name of the group chat</param>
        /// <param name="chatMember">Name of the chat member to check</param>
        /// <returns>true, if the group chat contains the given chat member</returns>
        bool IsMemberInGroupChat(string groupChatName, string chatMember);

        /// <summary>
        /// Adds a new member to the group chat
        /// </summary>
        /// <param name="groupChatName">Name of the group chat</param>
        /// <param name="chatMember">Name of the new member</param>
        void AddMember(string groupChatName, string chatMember);
    }
}