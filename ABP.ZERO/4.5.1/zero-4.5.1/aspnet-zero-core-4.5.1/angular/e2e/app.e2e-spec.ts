import { AbpZeroTemplatePage } from './app.po';

describe('abp-zero-template App', function () {
    let page: AbpZeroTemplatePage;

    beforeEach(() => {
        page = new AbpZeroTemplatePage();
    });

    it('should display message saying app works', () => {
        page.navigateTo();
        page.getCopyright().then(value => {
            expect(value).toEqual(new Date().getFullYear() + ' © AbpZeroTemplate.');
        });
    });
});
