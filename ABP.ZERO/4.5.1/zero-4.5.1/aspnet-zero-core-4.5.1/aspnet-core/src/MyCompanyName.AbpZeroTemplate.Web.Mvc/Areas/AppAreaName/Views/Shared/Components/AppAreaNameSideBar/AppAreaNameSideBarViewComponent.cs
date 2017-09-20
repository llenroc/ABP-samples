using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Navigation;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc;
using MyCompanyName.AbpZeroTemplate.MultiTenancy;
using MyCompanyName.AbpZeroTemplate.Web.Areas.AppAreaName.Models.Layout;
using MyCompanyName.AbpZeroTemplate.Web.Areas.AppAreaName.Startup;
using MyCompanyName.AbpZeroTemplate.Web.Views;

namespace MyCompanyName.AbpZeroTemplate.Web.Areas.AppAreaName.Views.Shared.Components.AppAreaNameSideBar
{
    public class AppAreaNameSideBarViewComponent : AbpZeroTemplateViewComponent
    {
        private readonly IUserNavigationManager _userNavigationManager;
        private readonly IAbpSession _abpSession;
        private readonly TenantManager _tenantManager;

        public AppAreaNameSideBarViewComponent(
            IUserNavigationManager userNavigationManager,
            IAbpSession abpSession,
            TenantManager tenantManager)
        {
            _userNavigationManager = userNavigationManager;
            _abpSession = abpSession;
            _tenantManager = tenantManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string currentPageName = null)
        {
            var sidebarModel = new SidebarViewModel
            {
                Menu = await _userNavigationManager.GetMenuAsync(AppAreaNameNavigationProvider.MenuName, _abpSession.ToUserIdentifier()),
                CurrentPageName = currentPageName
            };

            if (AbpSession.TenantId == null)
            {
                return View(sidebarModel);
            }

            var tenant = await _tenantManager.GetByIdAsync(AbpSession.TenantId.Value);
            if (tenant.EditionId.HasValue)
            {
                return View(sidebarModel);
            }

            var subscriptionManagement = FindMenuItemOrNull(sidebarModel.Menu.Items, AppAreaNamePageNames.Tenant.SubscriptionManagement);
            if (subscriptionManagement != null)
            {
                subscriptionManagement.IsVisible = false;
            }

            return View(sidebarModel);
        }

        public UserMenuItem FindMenuItemOrNull(IList<UserMenuItem> userMenuItems, string name)
        {
            if (userMenuItems == null)
            {
                return null;
            }

            foreach (var menuItem in userMenuItems)
            {
                if (menuItem.Name == name)
                {
                    return menuItem;
                }

                var found = FindMenuItemOrNull(menuItem.Items, name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
