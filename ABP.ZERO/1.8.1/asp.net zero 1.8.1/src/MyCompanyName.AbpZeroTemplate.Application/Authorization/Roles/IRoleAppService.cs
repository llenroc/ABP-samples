using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using MyCompanyName.AbpZeroTemplate.Authorization.Roles.Dto;

namespace MyCompanyName.AbpZeroTemplate.Authorization.Roles
{
    /// <summary>
    /// Application service that is used by 'role management' page.
    /// </summary>
    public interface IRoleAppService : IApplicationService
    {
        Task<ListResultOutput<RoleListDto>> GetRoles();

        Task<GetRoleForEditOutput> GetRoleForEdit(NullableIdInput input);

        Task CreateOrUpdateRole(CreateOrUpdateRoleInput input);

        Task DeleteRole(EntityRequestInput input);
    }
}