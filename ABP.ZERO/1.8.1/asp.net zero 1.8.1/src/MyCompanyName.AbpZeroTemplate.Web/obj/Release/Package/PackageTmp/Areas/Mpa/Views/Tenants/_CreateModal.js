(function ($) {
    //Custom validation type for tenancy name
    $.validator.addMethod("tenancyNameRegex", function (value, element, regexpr) {
        return regexpr.test(value);
    }, app.localize('TenantName_Regex_Description'));

    app.modals.CreateTenantModal = function() {
        var _tenantService = abp.services.app.tenant;
        var _$tenantInformationForm = null;

        var _modalManager;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$tenantInformationForm = _modalManager.getModal().find('form[name=TenantInformationsForm]');
            _$tenantInformationForm.validate({
                rules: {
                    TenancyName: {
                        tenancyNameRegex: new RegExp(_$tenantInformationForm.find('input[name=TenancyName]').attr('regex'))
                    }
                }
            });

            var passwordInputs = _modalManager.getModal().find('input[name=AdminPassword],input[name=AdminPasswordRepeat]');
            var passwordInputGroups = passwordInputs.closest('.form-group');

            $('#CreateTenant_SetRandomPassword').change(function () {
                if ($(this).is(':checked')) {
                    passwordInputGroups.slideUp('fast');
                    passwordInputs.removeAttr('required');
                } else {
                    passwordInputGroups.slideDown('fast');
                    passwordInputs.attr('required', 'required');
                }
            });
        };

        this.save = function () {
            if (!_$tenantInformationForm.valid()) {
                return;
            }

            var tenant = _$tenantInformationForm.serializeFormToObject();

            if (tenant.SetRandomPassword) {
                tenant.Password = null;
            }

            _modalManager.setBusy(true);
            _tenantService.createTenant(
                tenant
            ).done(function () {
                abp.notify.info(app.localize('SavedSuccessfully'));
                _modalManager.close();
                abp.event.trigger('app.createTenantModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);