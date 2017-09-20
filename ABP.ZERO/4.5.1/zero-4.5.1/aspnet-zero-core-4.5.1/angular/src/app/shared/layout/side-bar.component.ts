import { Component, Injector } from '@angular/core';
import { SideBarMenu } from './side-bar-menu';
import { SideBarMenuItem } from './side-bar-menu-item';

import { PermissionCheckerService } from '@abp/auth/permission-checker.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { AppSessionService } from '@shared/common/session/app-session.service';

@Component({
    templateUrl: './side-bar.component.html',
    selector: 'side-bar'
})
export class SideBarComponent extends AppComponentBase {

    constructor(
        injector: Injector,
        public permission: PermissionCheckerService,
        private _appSessionService: AppSessionService) {
        super(injector);
    }

    menu: SideBarMenu = new SideBarMenu("MainMenu", "MainMenu", [
        new SideBarMenuItem("Dashboard", "Pages.Administration.Host.Dashboard", "icon-home", "/app/admin/hostDashboard"),
        new SideBarMenuItem("Dashboard", "Pages.Tenant.Dashboard", "icon-home", "/app/main/dashboard"),
        new SideBarMenuItem("Tenants", "Pages.Tenants", "icon-globe", "/app/admin/tenants"),
        new SideBarMenuItem("Editions", "Pages.Editions", "icon-grid", "/app/admin/editions"),
        new SideBarMenuItem("Administration", "", "icon-wrench", "", [
            new SideBarMenuItem("OrganizationUnits", "Pages.Administration.OrganizationUnits", "icon-layers", "/app/admin/organization-units"),
            new SideBarMenuItem("Roles", "Pages.Administration.Roles", "icon-briefcase", "/app/admin/roles"),
            new SideBarMenuItem("Users", "Pages.Administration.Users", "icon-people", "/app/admin/users"),
            new SideBarMenuItem("Languages", "Pages.Administration.Languages", "icon-flag", "/app/admin/languages"),
            new SideBarMenuItem("AuditLogs", "Pages.Administration.AuditLogs", "icon-lock", "/app/admin/auditLogs"),
            new SideBarMenuItem("Maintenance", "Pages.Administration.Host.Maintenance", "icon-wrench", "/app/admin/maintenance"),
            new SideBarMenuItem("Subscription", "Pages.Administration.Tenant.SubscriptionManagement", "icon-refresh", "/app/admin/subscription-management"),
            new SideBarMenuItem("Settings", "Pages.Administration.Host.Settings", "icon-settings", "/app/admin/hostSettings"),
            new SideBarMenuItem("Settings", "Pages.Administration.Tenant.Settings", "icon-settings", "/app/admin/tenantSettings")
        ]),
        new SideBarMenuItem("DemoUiComponents", "Pages.DemoUiComponents", "icon-puzzle", "/app/admin/demo-ui-components"),
    ]);

    checkChildMenuItemPermission(menuItem): boolean {

        for (var i = 0; i < menuItem.items.length; i++) {
            var subMenuItem = menuItem.items[i];

            if (subMenuItem.permissionName && this.permission.isGranted(subMenuItem.permissionName)) {
                return true;
            }

            if (subMenuItem.items && subMenuItem.items.length) {
                return this.checkChildMenuItemPermission(subMenuItem);
            } else if (!subMenuItem.permissionName) {
                return true;
            }
        }

        return false;
    }

    showMenuItem(menuItem): boolean {
        if (menuItem.permissionName === 'Pages.Administration.Tenant.SubscriptionManagement' && this._appSessionService.tenant && !this._appSessionService.tenant.edition) {
            return false;
        }

        if (menuItem.permissionName) {
            return this.permission.isGranted(menuItem.permissionName);
        }

        if (menuItem.items && menuItem.items.length) {
            return this.checkChildMenuItemPermission(menuItem);
        }
        
        return true;
    }

}