import { Component, ViewChild, Injector, Output, EventEmitter, ElementRef } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { TenantServiceProxy, CommonLookupServiceProxy, TenantEditDto, SubscribableEditionComboboxItemDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';

import * as moment from "moment";
import * as _ from "lodash";

@Component({
    selector: 'editTenantModal',
    templateUrl: './edit-tenant-modal.component.html'
})
export class EditTenantModalComponent extends AppComponentBase {

    @ViewChild('nameInput') nameInput: ElementRef;
    @ViewChild('editModal') modal: ModalDirective;
    @ViewChild('SubscriptionEndDateUtc') subscriptionEndDateUtc: ElementRef;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active: boolean = false;
    saving: boolean = false;
    isUnlimited: boolean = false;
    subscriptionEndDateUtcIsValid: boolean = false;

    tenant: TenantEditDto = undefined;
    currentConnectionString: string;
    editions: SubscribableEditionComboboxItemDto[] = [];
    isSubscriptionFieldsVisible: boolean = false;

    constructor(
        injector: Injector,
        private _tenantService: TenantServiceProxy,
        private _commonLookupService: CommonLookupServiceProxy
    ) {
        super(injector);
    }

    show(tenantId: number): void {
        this.active = true;

        this._commonLookupService.getEditionsForCombobox(false).subscribe(result => {
            this.editions = result.items;
            var notSelectedEdition = new SubscribableEditionComboboxItemDto();
            notSelectedEdition.displayText = this.l('NotAssigned');
            notSelectedEdition.value = "0";
            this.editions.unshift(notSelectedEdition);

            this._tenantService.getTenantForEdit(tenantId).subscribe((result) => {
                this.tenant = result;
                this.currentConnectionString = result.connectionString;
                this.tenant.editionId = this.tenant.editionId || 0;
                this.isUnlimited = !this.tenant.subscriptionEndDateUtc;
                this.subscriptionEndDateUtcIsValid = this.isUnlimited || this.tenant.subscriptionEndDateUtc !== undefined;
                this.modal.show();
                this.toggleSubscriptionFields();
            });
        });
    }

    onShown(): void {
        $(this.nameInput.nativeElement).focus();
        $(this.subscriptionEndDateUtc.nativeElement).datetimepicker({
            locale: abp.localization.currentLanguage.name,
            format: 'L',
            defaultDate: this.tenant.subscriptionEndDateUtc,
        }).on("dp.change", (e) => {
            this.subscriptionEndDateUtcIsValid = e.date !== '';
        });
    }

    formatSubscriptionEndDate(date: any): string {
        if (this.isUnlimited) {
            return '';
        }
      
        if (!this.tenant.editionId) {
            return '';
        }

        if (!date) {
            return '';
        }

        return moment(date).format('L');
    }

    selectedEditionIsFree(): boolean {
        if (!this.tenant.editionId) {
            return true;
        }

        var selectedEditions = _.filter(this.editions, { value: this.tenant.editionId + "" });
        if (selectedEditions.length !== 1) {
            return true;
        }

        var selectedEdition = selectedEditions[0];
        return selectedEdition.isFree;
    }

    save(): void {
        this.saving = true;
        if (this.tenant.editionId == 0) {
            this.tenant.editionId = null;
        }

        //take selected date as UTC
        if (!this.isUnlimited && this.tenant.editionId) {
            var date = $(this.subscriptionEndDateUtc.nativeElement).data("DateTimePicker").date();
            if (!date) {
                date = this.tenant.subscriptionEndDateUtc;
            }

            this.tenant.subscriptionEndDateUtc = moment(date.format("YYYY-MM-DDTHH:mm:ss") + 'Z');
        } else {
            this.tenant.subscriptionEndDateUtc = null;
        }

        this._tenantService.updateTenant(this.tenant)
            .finally(() => this.saving = false)
            .subscribe(() => {
                this.notify.info(this.l('SavedSuccessfully'));
                this.close();
                this.modalSave.emit(null);
            });
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }

    onEditionChange(): void {
        if (this.selectedEditionIsFree()) {
            this.tenant.isInTrialPeriod = false;
        }

        this.toggleSubscriptionFields();
    }

    onUnlimitedChange(): void {
        if (this.isUnlimited) {
            $(this.subscriptionEndDateUtc.nativeElement).data("DateTimePicker").clear();
            this.tenant.subscriptionEndDateUtc = null;
            this.subscriptionEndDateUtcIsValid = true;
        } else {
            var date = $(this.subscriptionEndDateUtc.nativeElement).data("DateTimePicker").date();
            if (!date) {
                this.subscriptionEndDateUtcIsValid = false;
            }
        }
    }

    toggleSubscriptionFields() {
        if (this.tenant.editionId > 0) {
            this.isSubscriptionFieldsVisible = true;
        } else {
            this.isSubscriptionFieldsVisible = false;
        }
    }
}