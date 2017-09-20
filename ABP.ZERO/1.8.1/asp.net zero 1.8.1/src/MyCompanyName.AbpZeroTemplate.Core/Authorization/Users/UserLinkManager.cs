using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Uow;

namespace MyCompanyName.AbpZeroTemplate.Authorization.Users
{
    public class UserLinkManager : AbpZeroTemplateDomainServiceBase, IUserLinkManager
    {
        private readonly UserManager _userManager;

        public UserLinkManager(
            UserManager userManager)
        {
            _userManager = userManager;
        }


        [UnitOfWork]
        public virtual async Task Link(long userId, long targetUserId)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var user = await _userManager.GetUserByIdAsync(userId);
                var targetUser = await _userManager.GetUserByIdAsync(targetUserId);

                var userLinkId = user.UserLinkId ?? user.Id;
                user.UserLinkId = userLinkId;

                var usersToLink = targetUser.UserLinkId.HasValue
                    ? _userManager.Users.Where(u => u.UserLinkId == targetUser.UserLinkId.Value).ToList()
                    : new List<User> { targetUser };

                usersToLink.ForEach(u =>
                {
                    u.UserLinkId = userLinkId;
                });

                await CurrentUnitOfWork.SaveChangesAsync();
            }
        }

        public async Task<bool> AreUsersLinked(long firstUserId, long secondUserId)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var user1 = await _userManager.GetUserByIdAsync(firstUserId);
                var user2 = await _userManager.GetUserByIdAsync(secondUserId);

                if (!user1.UserLinkId.HasValue || !user2.UserLinkId.HasValue)
                {
                    return false;
                }

                return user1.UserLinkId == user2.UserLinkId;
            }
        }

        [UnitOfWork]
        public virtual async Task Unlink(long userId)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var targetUser = await _userManager.GetUserByIdAsync(userId);
                targetUser.UserLinkId = null;

                await CurrentUnitOfWork.SaveChangesAsync();
            }
        }
    }
}
