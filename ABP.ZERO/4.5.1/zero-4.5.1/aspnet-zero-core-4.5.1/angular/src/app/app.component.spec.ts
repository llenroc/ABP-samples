/* tslint:disable:no-unused-variable */

import { TestBed, async } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { RootModule } from '../root.module';
import { APP_BASE_HREF } from '@angular/common';

describe('App: AbpZeroTemplate', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            imports: [
                RootModule
            ],
            providers: [{ provide: APP_BASE_HREF, useValue: '/' }]
        }).compileComponents();
    }));

    it('should create the app', async(() => {
        let fixture = TestBed.createComponent(AppComponent);
        let app = fixture.debugElement.componentInstance;
        expect(app).toBeTruthy();
    }));

});