using System.Threading.Tasks;
using System.Web.Mvc;
using Abp.Web.Mvc.Authorization;
using MyCompanyName.AbpZeroTemplate.Authorization;
using MyCompanyName.AbpZeroTemplate.Configuration.Host;
using MyCompanyName.AbpZeroTemplate.Web.Controllers;

namespace MyCompanyName.AbpZeroTemplate.Web.Areas.Mpa.Controllers
{
    [AbpMvcAuthorize(AppPermissions.Pages_Administration_Host_Settings)]
    public class HostSettingsController : AbpZeroTemplateControllerBase
    {
        private readonly IHostSettingsAppService _hostSettingsAppService;

        public HostSettingsController(IHostSettingsAppService hostSettingsAppService)
        {
            _hostSettingsAppService = hostSettingsAppService;
        }

        public async Task<ActionResult> Index()
        {
            var output = await _hostSettingsAppService.GetAllSettings();

            return View(output);
        }
    }
}