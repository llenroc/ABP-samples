(function($) {
    $(function() {
        var _hostSettingsService = abp.services.app.hostSettings;

        var _$tabPanel = $('#SettingsTabPanel');

        var _$smtpCredentialFormGroups = _$tabPanel
            .find('input[name=SmtpDomain],input[name=SmtpUserName],input[name=SmtpPassword]')
            .closest('.form-group');

        function toggleSmtpCredentialFormGroups() {
            if ($('#Settings_SmtpUseDefaultCredentials').is(':checked')) {
                _$smtpCredentialFormGroups.slideUp('fast');
            } else {
                _$smtpCredentialFormGroups.slideDown('fast');
            }
        }

        $('#Settings_SmtpUseDefaultCredentials').change(function() {
            toggleSmtpCredentialFormGroups();
        });

        toggleSmtpCredentialFormGroups();

        $('#SaveAllSettingsButton').click(function() {
            _hostSettingsService.updateAllSettings({
                general: $('#GeneralSettingsForm').serializeFormToObject(),
                userManagement: $('#UserManagementSettingsForm').serializeFormToObject(),
                email: $('#EmailSmtpSettingsForm').serializeFormToObject()
            }).done(function() {
                abp.notify.info(app.localize('SavedSuccessfully'));
            });
        });
    });
})(jQuery);