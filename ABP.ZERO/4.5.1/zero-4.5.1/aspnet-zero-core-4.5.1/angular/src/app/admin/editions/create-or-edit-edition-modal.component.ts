import { Component, ViewChild, Injector, Output, EventEmitter, ElementRef } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { EditionServiceProxy, CommonLookupServiceProxy, EditionEditDto, CreateOrUpdateEditionDto, ComboboxItemDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { FeatureTreeComponent } from '../shared/feature-tree.component';
import { AppEditionExpireAction } from '@shared/AppEnums';

import * as _ from "lodash";

@Component({
    selector: 'createOrEditEditionModal',
    templateUrl: './create-or-edit-edition-modal.component.html'
})
export class CreateOrEditEditionModalComponent extends AppComponentBase {

    @ViewChild('editionNameInput') editionNameInput: ElementRef;
    @ViewChild('createOrEditModal') modal: ModalDirective;
    @ViewChild('featureTree') featureTree: FeatureTreeComponent;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active: boolean = false;
    saving: boolean = false;

    edition: EditionEditDto = new EditionEditDto();
    expiringEditions: ComboboxItemDto[] = [];

    expireAction: AppEditionExpireAction = AppEditionExpireAction.DeactiveTenant;
    expireActionEnum: typeof AppEditionExpireAction = AppEditionExpireAction;
    isFree: boolean = false;
    isTrialActive: boolean = false;
    isWaitingDayActive: boolean = false;

    constructor(
        injector: Injector,
        private _editionService: EditionServiceProxy,
        private _commonLookupService: CommonLookupServiceProxy
    ) {
        super(injector);
    }

    show(editionId?: number): void {
        this.active = true;

        this._commonLookupService.getEditionsForCombobox(true).subscribe(result => {
            this.expiringEditions = result.items;
            this.expiringEditions.unshift(new ComboboxItemDto({ value: null, displayText: this.l('NotAssigned'), isSelected: true }));

            this._editionService.getEditionForEdit(editionId).subscribe(result => {
                this.edition = result.edition;
                this.featureTree.editData = result;

                this.expireAction = this.edition.expiringEditionId > 0 ? AppEditionExpireAction.AssignToAnotherEdition : AppEditionExpireAction.DeactiveTenant;

                this.isFree = !result.edition.monthlyPrice && !result.edition.annualPrice;
                this.isTrialActive = result.edition.trialDayCount > 0;
                this.isWaitingDayActive = result.edition.waitingDayAfterExpire > 0;

                this.modal.show();
            });
        });
    }

    onShown(): void {
        $(this.editionNameInput.nativeElement).focus();
    }

    updateAnnualPrice(value): void {
        this.edition.annualPrice = value;
    }

    updateMonthlyPrice(value): void {
        this.edition.monthlyPrice = value;
    }

    resetPrices(isFree) {
        this.edition.annualPrice = undefined;
        this.edition.monthlyPrice = undefined;
    }

    removeExpiringEdition(isDeactivateTenant) {
        this.edition.expiringEditionId = null;
    }

    save(): void {
        const input = new CreateOrUpdateEditionDto();
        input.edition = this.edition;
        input.featureValues = this.featureTree.getGrantedFeatures();

        this.saving = true;
        this._editionService.createOrUpdateEdition(input)
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
}