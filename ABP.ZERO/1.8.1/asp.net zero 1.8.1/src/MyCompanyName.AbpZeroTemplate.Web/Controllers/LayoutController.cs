using System.Web.Mvc;
using Abp.Application.Navigation;
using Abp.Configuration.Startup;
using Abp.Threading;
using MyCompanyName.AbpZeroTemplate.Sessions;
using MyCompanyName.AbpZeroTemplate.Web.Models.Layout;
using MyCompanyName.AbpZeroTemplate.Web.Navigation;

namespace MyCompanyName.AbpZeroTemplate.Web.Controllers
{
    /// <summary>
    /// Layout for 'front end' pages.
    /// </summary>
    public class LayoutController : AbpZeroTemplateControllerBase
    {
        private readonly ISessionAppService _sessionAppService;
        private readonly IUserNavigationManager _userNavigationManager;
        private readonly IMultiTenancyConfig _multiTenancyConfig;

        public LayoutController(ISessionAppService sessionAppService, IUserNavigationManager userNavigationManager, IMultiTenancyConfig multiTenancyConfig)
        {
            _sessionAppService = sessionAppService;
            _userNavigationManager = userNavigationManager;
            _multiTenancyConfig = multiTenancyConfig;
        }

        [ChildActionOnly]
        public PartialViewResult Header(string currentPageName = "")
        {
            var headerModel = new HeaderViewModel();
            
            if (AbpSession.UserId.HasValue)
            {
                headerModel.LoginInformations = AsyncHelper.RunSync(() => _sessionAppService.GetCurrentLoginInformations());
            }

            headerModel.Languages = LocalizationManager.GetAllLanguages();
            headerModel.CurrentLanguage = LocalizationManager.CurrentLanguage;
            
            headerModel.Menu = AsyncHelper.RunSync(() => _userNavigationManager.GetMenuAsync(FrontEndNavigationProvider.MenuName, AbpSession.UserId));
            headerModel.CurrentPageName = currentPageName;

            headerModel.IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled;

            return PartialView("~/Views/Layout/_Header.cshtml", headerModel);
        }
    }
}