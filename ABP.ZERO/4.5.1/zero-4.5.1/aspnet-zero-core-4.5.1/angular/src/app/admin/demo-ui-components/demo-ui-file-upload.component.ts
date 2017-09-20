import { Component, Injector } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { DemoUiComponentsServiceProxy } from '@shared/service-proxies/service-proxies';
import { AppConsts } from '@shared/AppConsts';

@Component({
    selector: 'demo-ui-file-upload',
    templateUrl: './demo-ui-file-upload.component.html',
    animations: [appModuleAnimation()]
})

export class DemoUiFileUploadComponent extends AppComponentBase {

    uploadUrl: string;
    uploadedFiles: any[] = [];

    constructor(
        injector: Injector,
        private demoUiComponentsService: DemoUiComponentsServiceProxy
    ) {
        super(injector);
        this.uploadUrl = AppConsts.remoteServiceBaseUrl + '/DemoUiComponents/UploadFiles';
    }

    // upload completed event
    onUpload(event): void {
        for (let file of event.files) {
            this.uploadedFiles.push(file);
        }
    };
}