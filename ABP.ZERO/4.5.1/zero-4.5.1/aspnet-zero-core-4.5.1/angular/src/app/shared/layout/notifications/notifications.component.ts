import { Component, Injector, ViewChild, ChangeDetectorRef, ViewEncapsulation } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { NotificationServiceProxy, UserNotification } from '@shared/service-proxies/service-proxies';
import { UserNotificationHelper, IFormattedUserNotification } from './UserNotificationHelper';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppUserNotificationState } from '@shared/AppEnums';
import { DataTable } from 'primeng/components/datatable/datatable';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';

import * as moment from 'moment';

@Component({
    templateUrl: './notifications.component.html',
    styleUrls: ['./notifications.component.less'],
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class NotificationsComponent extends AppComponentBase {

    @ViewChild('dataTable') dataTable: DataTable;
    @ViewChild('paginator') paginator: Paginator;

    readStateFilter: string = 'ALL';
    loading: boolean = false;

    constructor(
        injector: Injector,
        private _notificationService: NotificationServiceProxy,
        private _userNotificationHelper: UserNotificationHelper,
        private changeDetectorRef: ChangeDetectorRef
    ) {
        super(injector);
    }

    reloadPage(): void {
        this.paginator.changePage(this.paginator.getPage(), null);
    }

    setAsRead(record: any): void {
        this.setNotificationAsRead(record, () => {
            this.reloadPage();
        });
    }

    isRead(record: any): boolean {
        return record.formattedNotification.state === 'READ';
    }

    fromNow(date: moment.Moment): string {
        return moment(date).fromNow();
    }

    formatRecord(record: any): IFormattedUserNotification {
        return this._userNotificationHelper.format(record, false);
    }

    formatNotification(record: any): string {
        var formattedRecord = this.formatRecord(record);
        return abp.utils.truncateStringWithPostfix(formattedRecord.text, 120);
    }

    formatNotifications(records: any[]): any[] {
        var formattedRecords = [];
        for (let record of records) {
            record.formattedNotification = this.formatRecord(record);
            formattedRecords.push(record);
        }
        return formattedRecords;
    }

    truncateString(text: any, length: number): string {
        return abp.utils.truncateStringWithPostfix(text, length);
    }

    getNotifications(event?: LazyLoadEvent): void {
        this.primengDatatableHelper.showLoadingIndicator();

        this._notificationService.getUserNotifications(
            this.readStateFilter === 'ALL' ? undefined : AppUserNotificationState.Unread,
            this.primengDatatableHelper.getMaxResultCount(this.paginator, event),
            this.primengDatatableHelper.getSkipCount(this.paginator, event)
        ).subscribe((result) => {
            this.primengDatatableHelper.totalRecordsCount = result.totalCount;
            this.primengDatatableHelper.records = this.formatNotifications(result.items);
            this.primengDatatableHelper.hideLoadingIndicator();
        });
    }

    setAllNotificationsAsRead(): void {
        this._userNotificationHelper.setAllAsRead(() => {
            this.getNotifications();
        });
    }

    openNotificationSettingsModal(): void {
        this._userNotificationHelper.openSettingsModal();
    }

    setNotificationAsRead(userNotification: UserNotification, callback: () => void): void {
        this._userNotificationHelper
            .setAsRead(userNotification.id, () => {
                callback && callback();
            });
    }

    private getRowClass(formattedRecord: IFormattedUserNotification): string {
        return formattedRecord.state === 'READ' ? 'notification-read' : '';
    }
}