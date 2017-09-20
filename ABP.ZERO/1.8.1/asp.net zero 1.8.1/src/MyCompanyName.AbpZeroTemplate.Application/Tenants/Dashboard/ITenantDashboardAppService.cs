using Abp.Application.Services;
using MyCompanyName.AbpZeroTemplate.Tenants.Dashboard.Dto;

namespace MyCompanyName.AbpZeroTemplate.Tenants.Dashboard
{
    public interface ITenantDashboardAppService : IApplicationService
    {
        GetMemberActivityOutput GetMemberActivity();
    }
}
