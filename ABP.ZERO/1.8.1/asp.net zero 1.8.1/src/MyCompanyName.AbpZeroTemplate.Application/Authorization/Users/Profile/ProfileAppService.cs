using System.Threading.Tasks;
using Abp.Auditing;
using Abp.Authorization;
using Abp.AutoMapper;
using MyCompanyName.AbpZeroTemplate.Authorization.Users.Profile.Dto;

namespace MyCompanyName.AbpZeroTemplate.Authorization.Users.Profile
{
    [AbpAuthorize]
    public class ProfileAppService : AbpZeroTemplateAppServiceBase, IProfileAppService
    {
        public async Task<CurrentUserProfileEditDto> GetCurrentUserProfileForEdit()
        {
            var user = await GetCurrentUserAsync();
            return user.MapTo<CurrentUserProfileEditDto>();
        }

        public async Task UpdateCurrentUserProfile(CurrentUserProfileEditDto input)
        {
            var user = await GetCurrentUserAsync();
            input.MapTo(user);
            CheckErrors(await UserManager.UpdateAsync(user));
        }

        [DisableAuditing]
        public async Task ChangePassword(ChangePasswordInput input)
        {
            var user = await GetCurrentUserAsync();
            CheckErrors(await UserManager.ChangePasswordAsync(user.Id, input.CurrentPassword, input.NewPassword));
        } 
    }
}