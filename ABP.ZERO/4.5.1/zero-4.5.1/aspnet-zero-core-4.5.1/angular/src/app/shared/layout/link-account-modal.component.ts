import { Component, ViewChild, Injector, Output, EventEmitter, ElementRef } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { UserLinkServiceProxy, LinkToUserInput } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { AppConsts } from '@shared/AppConsts';

@Component({
    selector: 'linkAccountModal',
    templateUrl: './link-account-modal.component.html'
})
export class LinkAccountModalComponent extends AppComponentBase {

    @ViewChild('tenancyNameInput') tenancyNameInput: ElementRef;
    @ViewChild('linkAccountModal') modal: ModalDirective;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active: boolean = false;
    saving: boolean = false;

    linkUser: LinkToUserInput = new LinkToUserInput();

    constructor(
        injector: Injector,
        private _userLinkService: UserLinkServiceProxy
    ) {
        super(injector);
    }

    show(): void {
        this.active = true;
        this.linkUser = new LinkToUserInput();
        this.modal.show();
    }

    onShown(): void {
        $(this.tenancyNameInput.nativeElement).focus();
    }

    save(): void {
        this.saving = true;
        this._userLinkService.linkToUser(this.linkUser)
            .finally(() => { this.saving = false; })
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