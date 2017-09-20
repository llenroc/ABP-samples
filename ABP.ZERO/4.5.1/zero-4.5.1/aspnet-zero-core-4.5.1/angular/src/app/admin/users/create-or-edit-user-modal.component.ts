import { Component, ViewChild, Injector, Output, EventEmitter, ElementRef } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { UserServiceProxy, ProfileServiceProxy, UserEditDto, CreateOrUpdateUserInput, OrganizationUnitDto, UserRoleDto, PasswordComplexitySetting } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { AppConsts } from '@shared/AppConsts';
import { OrganizationUnitsTreeComponent, IOrganizationUnitsTreeComponentData } from '../shared/organization-unit-tree.component';

import * as _ from "lodash";

@Component({
    selector: 'createOrEditUserModal',
    templateUrl: './create-or-edit-user-modal.component.html',
    styles: [`.user-edit-dialog-profile-image {
             margin-bottom: 20px;
        }`
    ]
})
export class CreateOrEditUserModalComponent extends AppComponentBase {
    
    @ViewChild('nameInput') nameInput: ElementRef;
    @ViewChild('createOrEditModal') modal: ModalDirective;
    @ViewChild('organizationUnitTree') organizationUnitTree: OrganizationUnitsTreeComponent;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active: boolean = false;
    saving: boolean = false;
    canChangeUserName: boolean = true;
    isTwoFactorEnabled: boolean = this.setting.getBoolean("Abp.Zero.UserManagement.TwoFactorLogin.IsEnabled");
    isLockoutEnabled: boolean = this.setting.getBoolean("Abp.Zero.UserManagement.UserLockOut.IsEnabled");
    passwordComplexitySetting: PasswordComplexitySetting = new PasswordComplexitySetting();

    user: UserEditDto = new UserEditDto();
    roles: UserRoleDto[];
    sendActivationEmail: boolean = true;
    setRandomPassword: boolean = true;
    passwordComplexityInfo: string = '';
    profilePicture: string;

    allOrganizationUnits: OrganizationUnitDto[];
    memberedOrganizationUnits: string[];

    constructor(
        injector: Injector,
        private _userService: UserServiceProxy,
        private _profileService: ProfileServiceProxy
    ) {
        super(injector);
    }

    show(userId?: number): void {
        if (!userId) {
            this.active = true;
            this.setRandomPassword = true;
            this.sendActivationEmail = true;
        }

        this._userService.getUserForEdit(userId).subscribe(result => {
            this.user = result.user;
            this.roles = result.roles;
            this.canChangeUserName = this.user.userName !== AppConsts.userManagement.defaultAdminUserName;

            this.allOrganizationUnits = result.allOrganizationUnits;
            this.memberedOrganizationUnits = result.memberedOrganizationUnits;

            this.getProfilePicture(result.profilePictureId);

            if (userId) {
                this.active = true;

                setTimeout(() => {
                    this.setRandomPassword = false;
                }, 0);

                this.sendActivationEmail = false;
            }

            this._profileService.getPasswordComplexitySetting().subscribe(result => {
                this.passwordComplexitySetting = result.setting;
                this.setPasswordComplexityInfo();
                this.modal.show();
            });
        });
    }

    setPasswordComplexityInfo(): void {

        this.passwordComplexityInfo = '<ul>';

        if (this.passwordComplexitySetting.requireDigit) {
            this.passwordComplexityInfo += '<li>' + this.l("PasswordComplexity_RequireDigit_Hint") + '</li>';
        }

        if (this.passwordComplexitySetting.requireLowercase) {
            this.passwordComplexityInfo += '<li>' + this.l("PasswordComplexity_RequireLowercase_Hint") + '</li>';
        }

        if (this.passwordComplexitySetting.requireUppercase) {
            this.passwordComplexityInfo += '<li>' + this.l("PasswordComplexity_RequireUppercase_Hint") + '</li>';
        }

        if (this.passwordComplexitySetting.requireNonAlphanumeric) {
            this.passwordComplexityInfo += '<li>' + this.l("PasswordComplexity_RequireNonAlphanumeric_Hint") + '</li>';
        }

        if (this.passwordComplexitySetting.requiredLength) {
            this.passwordComplexityInfo += '<li>' + this.l("PasswordComplexity_RequiredLength_Hint", this.passwordComplexitySetting.requiredLength) + '</li>';
        }

        this.passwordComplexityInfo += '</ul>';
    }

    getProfilePicture(profilePictureId: string): void {
        if (!profilePictureId) {
            this.profilePicture = "/assets/common/images/default-profile-picture.png";
        } else {
            this._profileService.getProfilePictureById(profilePictureId).subscribe(result => {

                if (result && result.profilePicture) {
                    this.profilePicture = 'data:image/jpeg;base64,' + result.profilePicture;
                } else {
                    this.profilePicture = "/assets/common/images/default-profile-picture.png";
                }
            });
        }
    }

    onShown(): void {
        $(this.nameInput.nativeElement).focus();

        this.organizationUnitTree.data = <IOrganizationUnitsTreeComponentData>{
            allOrganizationUnits : this.allOrganizationUnits,
            selectedOrganizationUnits: this.memberedOrganizationUnits
        };
    }

    save(): void {
        var input = new CreateOrUpdateUserInput();

        input.user = this.user;
        input.setRandomPassword = this.setRandomPassword;
        input.sendActivationEmail = this.sendActivationEmail;
        input.assignedRoleNames =
            _.map(
                _.filter(this.roles, { isAssigned: true }), role => role.roleName
            );

        input.organizationUnits = this.organizationUnitTree.getSelectedOrganizations();

        this.saving = true;
        this._userService.createOrUpdateUser(input)
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

    getAssignedRoleCount(): number {
        return _.filter(this.roles, { isAssigned: true }).length;
    }
}
