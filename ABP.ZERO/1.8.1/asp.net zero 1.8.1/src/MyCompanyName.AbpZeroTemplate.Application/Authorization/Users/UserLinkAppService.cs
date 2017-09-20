using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Abp.UI;
using MyCompanyName.AbpZeroTemplate.Authorization.Users.Dto;

namespace MyCompanyName.AbpZeroTemplate.Authorization.Users
{
    [AbpAuthorize]
    public class UserLinkAppService : AbpZeroTemplateAppServiceBase, IUserLinkAppService
    {
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly IUserLinkManager _userLinkManager;

        public UserLinkAppService(
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            IUserLinkManager userLinkManager)
        {
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _userLinkManager = userLinkManager;
        }

        public async Task LinkToUser(LinkToUserInput input)
        {
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var loginResult = await UserManager.LoginAsync(input.UsernameOrEmailAddress, input.Password, input.TenancyName);

                if (loginResult.Result != AbpLoginResultType.Success)
                {
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, input.UsernameOrEmailAddress, input.TenancyName);
                }

                if (loginResult.User.Id == AbpSession.GetUserId())
                {
                    throw new UserFriendlyException(L("YouCannotLinkToSameAccount"));
                }

                if (loginResult.User.ShouldChangePasswordOnNextLogin)
                {
                    throw new UserFriendlyException(L("ChangePasswordBeforeLinkToAnAccount"));
                }

                await _userLinkManager.Link(AbpSession.GetUserId(), loginResult.User.Id);
            }
        }

        public async Task<PagedResultOutput<LinkedUserDto>> GetLinkedUsers(GetLinkedUsersInput input)
        {
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var query = CreateLinkedUsersQuery(input.Sorting)
                            .Skip(input.SkipCount)
                            .Take(input.MaxResultCount);

                var totalCount = await query.CountAsync();
                var linkedUsers = await query.ToListAsync();

                return new PagedResultOutput<LinkedUserDto>(
                    totalCount,
                    linkedUsers
                );
            }
        }

        public async Task<ListResultOutput<LinkedUserDto>> GetRecentlyUsedLinkedUsers()
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var query = CreateLinkedUsersQuery("LastLoginTime DESC");
                var recentlyUsedlinkedUsers = await query.Skip(0).Take(3).ToListAsync();

                return new ListResultOutput<LinkedUserDto>(recentlyUsedlinkedUsers);
            }
        }

        public async Task UnlinkUser(UnlinkUserInput input)
        {
            var currentUserId = AbpSession.GetUserId();
            var currentUser = await UserManager.GetUserByIdAsync(currentUserId);

            if (!currentUser.UserLinkId.HasValue)
            {
                throw new ApplicationException(L("You are not linked to any account"));
            }

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                if (!await _userLinkManager.AreUsersLinked(currentUserId, input.UserId))
                {
                    return;
                }

                await _userLinkManager.Unlink(input.UserId);
            }
        }

        private IQueryable<LinkedUserDto> CreateLinkedUsersQuery(string sorting)
        {
            var currentUserId = AbpSession.GetUserId();
            var currentUser = UserManager.Users.Single(u => u.Id == currentUserId);

            return UserManager.Users.Include(user => user.Tenant)
                .Where(user => user.UserLinkId.HasValue && user.Id != currentUserId && user.UserLinkId == currentUser.UserLinkId )
                .OrderBy(sorting)
                .Select(user => new LinkedUserDto
                {
                    Id = user.Id,
                    ProfilePictureId = user.ProfilePictureId,
                    TenancyName = user.Tenant.TenancyName,
                    Username = user.UserName
                });
        }

    }
}