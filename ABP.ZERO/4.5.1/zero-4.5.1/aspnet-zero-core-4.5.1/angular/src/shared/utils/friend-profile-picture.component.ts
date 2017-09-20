﻿import { Component, ElementRef, Input, AfterViewInit } from '@angular/core';
import { ProfileServiceProxy } from '@shared/service-proxies/service-proxies';

@Component({
    selector: 'friend-profile-picture',
    template:
    `<img [src]="profilePicture" class="{{cssClass}}" alt="...">`
})
export class FriendProfilePictureComponent implements AfterViewInit {

    @Input() profilePictureId: string;
    @Input() userId: number;
    @Input() tenantId: number;
    @Input() cssClass: string = 'media-object';

    profilePicture: string = "/assets/common/images/default-profile-picture.png";

    constructor(
        private _profileService: ProfileServiceProxy
    ) {
    }

    ngAfterViewInit(): void {
        this.setProfileImage();
    }

    private setProfileImage(): void {
        if (!this.profilePictureId) {
            this.profilePictureId = undefined;
        }

        if (!this.tenantId) {
            this.tenantId = undefined;
        }

        this._profileService.getFriendProfilePictureById(this.profilePictureId, this.userId, this.tenantId).subscribe((result) => {
            if (result && result.profilePicture) {
                this.profilePicture = 'data:image/jpeg;base64,' + result.profilePicture;
            }
        });
    }
}