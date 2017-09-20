import { Component, Injector } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { DemoUiComponentsServiceProxy, NameValueOfString } from '@shared/service-proxies/service-proxies';

@Component({
    selector: 'demo-ui-selection',
    templateUrl: './demo-ui-selection.component.html',
    animations: [appModuleAnimation()]
})

export class DemoUiSelectionComponent extends AppComponentBase {

    filteredCountries: NameValueOfString[];
    country: any;
    countries: NameValueOfString[] = new Array<NameValueOfString>();

    constructor(
        injector: Injector,
        private demoUiComponentsService: DemoUiComponentsServiceProxy
    ) {
        super(injector);
    }

    // get countries
    filterCountries(event): void {
        this.demoUiComponentsService.getCountries(event.query).subscribe(countries => {
            this.filteredCountries = countries;
        });
    };

    // single select - post
    submitSelectedCountry(): void {
        var selectedCountries = new Array<NameValueOfString>();

        selectedCountries.push(this.country);

        this.demoUiComponentsService.sendAndGetSelectedCountries(selectedCountries)
            .subscribe((countries: NameValueOfString[]) => {
                var message = "";

                $.each(countries, (index, item) => {
                    message += `<div><strong>id</strong>: ${item.value} - <strong>name</strong>: ${item.name}</div>`;
                });

                (abp as any).libs.sweetAlert.config.info.html = true;
                this.message.info(message, this.l('PostedValue'));
                this.notify.info(this.l('SavedSuccessfully'));
            });
    };

    // multi select - post
    submitSelectedCountries(): void {
        this.demoUiComponentsService.sendAndGetSelectedCountries(this.countries)
            .subscribe((countries: NameValueOfString[]) => {
                var message = "";

                $.each(countries, (index, item) => {
                    message += `<div><strong>id</strong>: ${item.value} - <strong>name</strong>: ${item.name}</div>`;
                });

                (abp as any).libs.sweetAlert.config.info.html = true;
                this.message.info(message, this.l('PostedValue'));
                this.notify.info(this.l('SavedSuccessfully'));
            });
    };
}