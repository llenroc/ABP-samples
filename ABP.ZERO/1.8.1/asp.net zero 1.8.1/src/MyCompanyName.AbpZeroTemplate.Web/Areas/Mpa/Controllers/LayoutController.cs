using System.Web.Mvc;
using Abp.Application.Navigation;
using Abp.Configuration.Startup;
using Abp.Threading;
using Abp.Web.Mvc.Authorization;
using MyCompanyName.AbpZeroTemplate.Authorization.Users;
using MyCompanyName.AbpZeroTemplate.Sessions;
using MyCompanyName.AbpZeroTemplate.Web.Areas.Mpa.Models.Layout;
using MyCompanyName.AbpZeroTemplate.Web.Areas.Mpa.Startup;
using MyCompanyName.AbpZeroTemplate.Web.Controllers;

namespace MyCompanyName.AbpZeroTemplate.Web.Areas.Mpa.Controllers
{
    [AbpMvcAuthorize]
    public class LayoutController : AbpZeroTemplateControllerBase
    {
        private readonly ISessionAppService _sessionAppService;
        private readonly IUserNavigationManager _userNavigationManager;
        private readonly IMultiTenancyConfig _multiTenancyConfig;

        public LayoutController(
            ISessionAppService sessionAppService, 
            IUserNavigationManager userNavigationManager, 
            IMultiTenancyConfig multiTenancyConfig, 
            IUserLinkAppService userLinkAppService)
        {
            _sessionAppService = sessionAppService;
            _userNavigationManager = userNavigationManager;
            _multiTenancyConfig = multiTenancyConfig;
        }

        [ChildActionOnly]
        public PartialViewResult Header()
        {
            var headerModel = new HeaderViewModel
            {
                LoginInformations = AsyncHelper.RunSync(() => _sessionAppService.GetCurrentLoginInformations()),
                Languages = LocalizationManager.GetAllLanguages(),
                CurrentLanguage = LocalizationManager.CurrentLanguage,
                IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled,
                IsImpersonatedLogin = AbpSession.ImpersonatorUserId.HasValue
            };

            return PartialView("_Header", headerModel);
        }

        [ChildActionOnly]
        public PartialViewResult Sidebar(string currentPageName = "")
        {
            var sidebarModel = new SidebarViewModel
            {
                Menu = AsyncHelper.RunSync(() => _userNavigationManager.GetMenuAsync(MpaNavigationProvider.MenuName, AbpSession.UserId)),
                CurrentPageName = currentPageName
            };

            return PartialView("_Sidebar", sidebarModel);
        }

        [ChildActionOnly]
        public PartialViewResult Footer()
        {
            var footerModel = new FooterViewModel
            {
                LoginInformations = AsyncHelper.RunSync(() => _sessionAppService.GetCurrentLoginInformations())
            };

            return PartialView("_Footer", footerModel);
        }
    }
}