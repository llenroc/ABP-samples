import { Component, OnInit, Injector, ViewEncapsulation, ViewChild } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { HostSettingsServiceProxy, HostSettingsEditDto, CommonLookupServiceProxy, ComboboxItemDto, DefaultTimezoneScope, SendTestEmailInput } from '@shared/service-proxies/service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppConsts } from '@shared/AppConsts';
import { AppComponentBase } from '@shared/common/app-component-base';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { AppTimezoneScope  } from '@shared/AppEnums';
import { appModuleAnimation } from '@shared/animations/routerTransition';

import * as moment from "moment";

@Component({
    templateUrl: "./host-settings.component.html",
    animations: [appModuleAnimation()]
})
export class HostSettingsComponent extends AppComponentBase implements OnInit {

    loading: boolean = false;
    hostSettings: HostSettingsEditDto;
    editions: ComboboxItemDto[] = undefined;
    testEmailAddress: string = undefined;
    showTimezoneSelection = abp.clock.provider.supportsMultipleTimezone;
    defaultTimezoneScope: DefaultTimezoneScope = AppTimezoneScope.Application;

    usingDefaultTimeZone: boolean = false;
    initialTimeZone: string = undefined;

    constructor(
        injector: Injector,
        private _hostSettingService: HostSettingsServiceProxy,
        private _commonLookupService: CommonLookupServiceProxy,
        private _appSessionService: AppSessionService
    ) {
        super(injector);
    }

    loadHostSettings(): void {
        let self = this;
        self._hostSettingService.getAllSettings()
            .subscribe(setting => {
                self.hostSettings = setting;
                self.initialTimeZone = setting.general.timezone;
                self.usingDefaultTimeZone = setting.general.timezoneForComparison === self.setting.get("Abp.Timing.TimeZone");
            });
    }

    loadEditions(): void {
        let self = this;
        self._commonLookupService.getEditionsForCombobox(false).subscribe((result) => {
            self.editions = result.items;

            let notAssignedEdition = new ComboboxItemDto();
            notAssignedEdition.value = null;
            notAssignedEdition.displayText = self.l("NotAssigned");

            self.editions.unshift(notAssignedEdition);
        });
    }

    init(): void {
        let self = this;
        self.testEmailAddress = self._appSessionService.user.emailAddress;
        self.showTimezoneSelection = abp.clock.provider.supportsMultipleTimezone;
        self.loadHostSettings();
        self.loadEditions();
    }

    ngOnInit(): void {
        let self = this;
        self.init();
    }

    sendTestEmail(): void {
        let self = this;
        let input = new SendTestEmailInput();
        input.emailAddress = self.testEmailAddress;
        self._hostSettingService.sendTestEmail(input).subscribe(result => {
            self.notify.info(self.l("TestEmailSentSuccessfully"));
        });
    };

    saveAll(): void {
        let self = this;
        self._hostSettingService.updateAllSettings(self.hostSettings).subscribe(result => {
            self.notify.info(self.l('SavedSuccessfully'));

            if (abp.clock.provider.supportsMultipleTimezone && self.usingDefaultTimeZone && self.initialTimeZone !== self.hostSettings.general.timezone) {
                self.message.info(self.l('TimeZoneSettingChangedRefreshPageNotification')).done(function () {
                    window.location.reload();
                });
            }

           // this.setting.set('App.UserManagement.SmsVerificationEnabled','true');
        });
    }
}