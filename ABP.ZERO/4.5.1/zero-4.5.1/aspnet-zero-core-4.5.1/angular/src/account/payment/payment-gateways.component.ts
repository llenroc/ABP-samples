import { Component, Input } from '@angular/core';
import { PayPalComponent } from './paypal/paypal.component';
import { EditionSelectDto } from '@shared/service-proxies/service-proxies';
import { PaymentPeriodType, EditionPaymentType } from "@shared/AppEnums";

@Component({
    selector: 'paymentGateways',
    entryComponents: [PayPalComponent],
    template: `
    <paypal-component [(edition)]='edition' [(selectedPaymentPeriodType)]='paymentPeriodType' [(editionPaymentType)]='editionPaymentType'></paypal-component>
  `
})
export class PaymentGatewaysComponent {
    @Input() edition: EditionSelectDto = null;
    @Input() paymentPeriodType: PaymentPeriodType = null;
    @Input() editionPaymentType: EditionPaymentType = null;
}
