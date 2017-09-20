using System.Threading.Tasks;
using Abp.Configuration;

namespace MyCompanyName.AbpZeroTemplate.Timing
{
    public interface ITimeZoneService
    {
        Task<string> GetDefaultTimezoneAsync(SettingScopes scope, int? tenantId);
    }
}
