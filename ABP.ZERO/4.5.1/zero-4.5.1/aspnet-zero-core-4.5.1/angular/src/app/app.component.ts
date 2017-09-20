import { Component, ViewContainerRef, OnInit, AfterViewInit, Injector } from '@angular/core';
import { ChatSignalrService } from 'app/shared/layout/chat/chat-signalr.service';
import { SignalRHelper } from 'shared/helpers/SignalRHelper';
import { AppComponentBase } from 'shared/common/app-component-base';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { SubscriptionStartType } from "@shared/AppEnums";
import { AppConsts } from '@shared/AppConsts';
import * as moment from 'moment';

@Component({
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.less']
})
export class AppComponent extends AppComponentBase implements OnInit, AfterViewInit {

    private viewContainerRef: ViewContainerRef;
    subscriptionStartType = SubscriptionStartType;

    public constructor(
        injector: Injector,
        viewContainerRef: ViewContainerRef,
        private _chatSignalrService: ChatSignalrService,
        private _appSessionService: AppSessionService) {
        super(injector);
        this.viewContainerRef = viewContainerRef; // You need this small hack in order to catch application root view container ref (required by ng2 bootstrap modal)
    }

    ngOnInit(): void {
        if (this.appSession.application.features['SignalR']) {
            SignalRHelper.initSignalR(() => { this._chatSignalrService.init(); });
        }
    }

    subscriptionStatusBarVisible(): boolean {
        return this._appSessionService.tenantId > 0 &&
            (this._appSessionService.tenant.isInTrialPeriod ||
                this.subscriptionIsExpiringSoon());
    }

    subscriptionIsExpiringSoon(): boolean {
        if (this._appSessionService.tenant.subscriptionEndDateUtc) {
            return moment().utc().add(AppConsts.subscriptionExpireNootifyDayCount, 'days') >= moment(this._appSessionService.tenant.subscriptionEndDateUtc);
        }

        return false;
    }

    ngAfterViewInit(): void {
        if (App.initialized) {
            return;
        }

        App.init();
        App.initComponents();
        Layout.init();
        App.initialized = true;
    }
}