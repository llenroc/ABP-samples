import { Component, ViewChild, Injector, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { AuditLogListDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { AppConsts } from '@shared/AppConsts';

import * as moment from 'moment';

@Component({
    selector: 'auditLogDetailModal',
    templateUrl: './audit-log-detail-modal.component.html'
})
export class AuditLogDetailModalComponent extends AppComponentBase {

    @ViewChild('auditLogDetailModal') modal: ModalDirective;

    active: boolean = false;
    auditLog: AuditLogListDto;

    constructor(
        injector: Injector
    ) {
        super(injector);
    }

    getExecutionTime(): string {
        let self = this;
        return moment(self.auditLog.executionTime).fromNow() + ' (' + moment(self.auditLog.executionTime).format('YYYY-MM-DD HH:mm:ss') + ')';
    };

    getDurationAsMs(): string {
        let self = this;
        return self.l('Xms', self.auditLog.executionDuration);
    };

    getFormattedParameters(): string {
        let self = this;
        try {
            var json = JSON.parse(self.auditLog.parameters);
            return JSON.stringify(json, null, 4);
        } catch (e) {
            return self.auditLog.parameters;
        }
    }

    show(record: AuditLogListDto): void {
        let self = this;
        self.active = true;
        self.auditLog = record;

        self.modal.show();
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }
}