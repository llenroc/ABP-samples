using Abp.Application.Navigation;
using Abp.Localization;
using MyCompanyName.AbpZeroTemplate.Authorization;

namespace MyCompanyName.AbpZeroTemplate.Web.Areas.AppAreaName.Startup
{
    public class AppAreaNameNavigationProvider : NavigationProvider
    {
        public const string MenuName = "App";

        public override void SetNavigation(INavigationProviderContext context)
        {
            var menu = context.Manager.Menus[MenuName] = new MenuDefinition(MenuName, new FixedLocalizableString("Main Menu"));

            menu
                .AddItem(new MenuItemDefinition(
                        AppAreaNamePageNames.Host.Dashboard,
                        L("Dashboard"),
                        url: "AppAreaName/HostDashboard",
                        icon: "icon-home",
                        requiredPermissionName: AppPermissions.Pages_Administration_Host_Dashboard
                    )
                ).AddItem(new MenuItemDefinition(
                    AppAreaNamePageNames.Host.Tenants,
                    L("Tenants"),
                    url: "AppAreaName/Tenants",
                    icon: "icon-globe",
                    requiredPermissionName: AppPermissions.Pages_Tenants
                    )
                ).AddItem(new MenuItemDefinition(
                        AppAreaNamePageNames.Host.Editions,
                        L("Editions"),
                        url: "AppAreaName/Editions",
                        icon: "icon-grid",
                        requiredPermissionName: AppPermissions.Pages_Editions
                    )
                ).AddItem(new MenuItemDefinition(
                        AppAreaNamePageNames.Tenant.Dashboard,
                        L("Dashboard"),
                        url: "AppAreaName/Dashboard",
                        icon: "icon-home",
                        requiredPermissionName: AppPermissions.Pages_Tenant_Dashboard
                    )
                ).AddItem(new MenuItemDefinition(
                        AppAreaNamePageNames.Common.Administration,
                        L("Administration"),
                        icon: "icon-wrench"
                    ).AddItem(new MenuItemDefinition(
                            AppAreaNamePageNames.Common.OrganizationUnits,
                            L("OrganizationUnits"),
                            url: "AppAreaName/OrganizationUnits",
                            icon: "icon-layers",
                            requiredPermissionName: AppPermissions.Pages_Administration_OrganizationUnits
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppAreaNamePageNames.Common.Roles,
                            L("Roles"),
                            url: "AppAreaName/Roles",
                            icon: "icon-briefcase",
                            requiredPermissionName: AppPermissions.Pages_Administration_Roles
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppAreaNamePageNames.Common.Users,
                            L("Users"),
                            url: "AppAreaName/Users",
                            icon: "icon-people",
                            requiredPermissionName: AppPermissions.Pages_Administration_Users
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppAreaNamePageNames.Common.Languages,
                            L("Languages"),
                            url: "AppAreaName/Languages",
                            icon: "icon-flag",
                            requiredPermissionName: AppPermissions.Pages_Administration_Languages
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppAreaNamePageNames.Common.AuditLogs,
                            L("AuditLogs"),
                            url: "AppAreaName/AuditLogs",
                            icon: "icon-lock",
                            requiredPermissionName: AppPermissions.Pages_Administration_AuditLogs
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppAreaNamePageNames.Host.Maintenance,
                            L("Maintenance"),
                            url: "AppAreaName/Maintenance",
                            icon: "icon-wrench",
                            requiredPermissionName: AppPermissions.Pages_Administration_Host_Maintenance
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppAreaNamePageNames.Tenant.SubscriptionManagement,
                            L("Subscription"),
                            url: "AppAreaName/SubscriptionManagement",
                            icon: "icon-refresh"
                            ,
                            requiredPermissionName: AppPermissions.Pages_Administration_Tenant_SubscriptionManagement
                        )
                    )
                    .AddItem(new MenuItemDefinition(
                            AppAreaNamePageNames.Host.Settings,
                            L("Settings"),
                            url: "AppAreaName/HostSettings",
                            icon: "icon-settings",
                            requiredPermissionName: AppPermissions.Pages_Administration_Host_Settings
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppAreaNamePageNames.Tenant.Settings,
                            L("Settings"),
                            url: "AppAreaName/Settings",
                            icon: "icon-settings",
                            requiredPermissionName: AppPermissions.Pages_Administration_Tenant_Settings
                        )
                    )
                ).AddItem(new MenuItemDefinition(
                        AppAreaNamePageNames.Common.DemoUiComponents,
                        L("DemoUiComponents"),
                        url: "AppAreaName/DemoUiComponents",
                        icon: "icon-puzzle",
                        requiredPermissionName: AppPermissions.Pages_DemoUiComponents
                    )
                );
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, AbpZeroTemplateConsts.LocalizationSourceName);
        }
    }
}