using System.Threading.Tasks;

namespace MyCompanyName.AbpZeroTemplate.Authorization.Users
{
    public interface IUserLinkManager
    {
        Task Link(long userId, long targetUserId);

        /// <summary>
        /// returns true if user1 is linked to user2
        /// </summary>
        /// <param name="firstUserId"></param>
        /// <param name="secondUserId"></param>
        /// <returns></returns>
        Task<bool> AreUsersLinked(long firstUserId, long secondUserId);

        Task Unlink(long userId);
    }
}