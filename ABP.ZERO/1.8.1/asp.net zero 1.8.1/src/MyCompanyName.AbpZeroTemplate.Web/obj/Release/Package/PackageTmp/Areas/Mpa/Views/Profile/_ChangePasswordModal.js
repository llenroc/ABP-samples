(function($) {
    app.modals.ChangePasswordModal = function () {

        var _profileService = abp.services.app.profile;

        var _modalManager;
        var _$form = null;

        this.init = function(modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form[name=ChangePasswordModalForm]');
            _$form.validate();
        };

        this.save = function () {
            if (!_$form.valid()) {
                return;
            }

            _modalManager.setBusy(true);
            _profileService.changePassword(_$form.serializeFormToObject())
                .done(function () {
                    abp.notify.info(app.localize('YourPasswordHasChangedSuccessfully'));
                    _modalManager.close();
                }).always(function () {
                    _modalManager.setBusy(false);
                });
        };
    };
})(jQuery);