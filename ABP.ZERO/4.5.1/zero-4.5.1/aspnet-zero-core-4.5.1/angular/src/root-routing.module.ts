import { NgModule } from '@angular/core';
import { Routes, RouterModule, Router, NavigationEnd } from '@angular/router';

const routes: Routes = [
    { path: '', redirectTo: '/app/main/dashboard', pathMatch: 'full' },
    {
        path: 'account',
        loadChildren: 'account/account.module#AccountModule', //Lazy load account module
        data: { preload: true }
    }
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule],
    providers: []
})
export class RootRoutingModule {
    constructor(private router: Router) {
        router.events.subscribe((event: NavigationEnd) => {
            setTimeout(() => {
                this.toggleBodyCssClass(event.url);
            }, 0);
        });
    }

    toggleBodyCssClass(url: string): void {
        if (url) {

            if (url === '/') {
                if (abp.session.userId > 0) {
                    $('body').attr('class', 'page-md page-header-fixed page-sidebar-closed-hide-logo');
                } else {
                    $('body').attr('class', 'page-md login');
                }
            }

            if (url.indexOf("/account/") >= 0) {
                $('body').attr('class', 'page-md login');
            } else {
                $('body').attr('class', 'page-md page-header-fixed page-sidebar-closed-hide-logo');
            }
        }
    }
}