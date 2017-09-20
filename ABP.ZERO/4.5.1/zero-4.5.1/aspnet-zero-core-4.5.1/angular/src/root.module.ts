import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule, Injector, APP_INITIALIZER, LOCALE_ID } from '@angular/core';

import { AbpModule, ABP_HTTP_PROVIDER } from '@abp/abp.module';

import { AppModule } from './app/app.module';
import { CommonModule } from '@shared/common/common.module';
import { ServiceProxyModule } from '@shared/service-proxies/service-proxy.module';
import { RootRoutingModule } from './root-routing.module';

import { AppConsts } from '@shared/AppConsts';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { API_BASE_URL } from '@shared/service-proxies/service-proxies';

import { RootComponent } from './root.component';
import { AppPreBootstrap } from './AppPreBootstrap';

import { UrlHelper } from '@shared/helpers/UrlHelper';
import { AppAuthService } from '@app/shared/common/auth/app-auth.service';

export function appInitializerFactory(injector: Injector) {
    return () => {
        abp.ui.setBusy();

        handleLogoutRequest(injector.get(AppAuthService));

        return new Promise<boolean>((resolve, reject) => {
            AppPreBootstrap.run(() => {
                var appSessionService: AppSessionService = injector.get(AppSessionService);
                appSessionService.init().then(
                    (result) => {

                        //Css classes based on the layout
                        if (abp.session.userId) {
                            $('body').attr('class', 'page-md page-header-fixed page-sidebar-closed-hide-logo');
                        } else {
                            $('body').attr('class', 'page-md login');
                        }

                        //tenant specific custom css
                        if (appSessionService.tenant && appSessionService.tenant.customCssId) {
                            $('head').append('<link id="TenantCustomCss" href="' + AppConsts.remoteServiceBaseUrl + '/TenantCustomization/GetCustomCss?id=' + appSessionService.tenant.customCssId + '" rel="stylesheet"/>');
                        }

                        abp.ui.clearBusy();
                        resolve(result);
                    },
                    (err) => {
                        abp.ui.clearBusy();
                        reject(err);
                    }
                );
            });
        });
    }
}

export function getRemoteServiceBaseUrl(): string {
    return AppConsts.remoteServiceBaseUrl;
}

export function getCurrentLanguage(): string {
    return abp.localization.currentLanguage.name;
}

function handleLogoutRequest(authService: AppAuthService) {
    var currentUrl = UrlHelper.initialUrl;
    var returnUrl = UrlHelper.getReturnUrl();
    if (currentUrl.indexOf(('account/logout')) >= 0 && returnUrl) {
        authService.logout(true, returnUrl);
    }
}

@NgModule({
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        AppModule,
        CommonModule.forRoot(),
        AbpModule,
        ServiceProxyModule,

        RootRoutingModule
    ],
    declarations: [
        RootComponent
    ],
    providers: [
        ABP_HTTP_PROVIDER,
        { provide: API_BASE_URL, useFactory: getRemoteServiceBaseUrl },
        {
            provide: APP_INITIALIZER,
            useFactory: appInitializerFactory,
            deps: [Injector],
            multi: true
        },
        {
            provide: LOCALE_ID,
            useFactory: getCurrentLanguage
        }
    ],
    bootstrap: [RootComponent]
})
export class RootModule {

}