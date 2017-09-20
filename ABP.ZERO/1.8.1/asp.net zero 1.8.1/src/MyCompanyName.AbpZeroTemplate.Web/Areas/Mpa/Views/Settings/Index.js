(function() {
    $(function () {
        var _tenantSettingsService = abp.services.app.tenantSettings;

        //Toggle form based registration options
        var _$selfRegistrationOptions = $('#FormBasedRegistrationSettingsForm')
            .find('input[name=IsNewRegisteredUserActiveByDefault],input[name=UseCaptchaOnRegistration]')
            .closest('.md-checkbox');
        
        function toggleSelfRegistrationOptions() {
            if ($('#Setting_AllowSelfRegistration').is(':checked')) {
                _$selfRegistrationOptions.slideDown('fast');
            } else {
                _$selfRegistrationOptions.slideUp('fast');
            }
        }

        $('#Setting_AllowSelfRegistration').change(function () {
            toggleSelfRegistrationOptions();
        });

        toggleSelfRegistrationOptions();

        //Toggle SMTP credentials
        var _$smtpCredentialFormGroups = $('#EmailSmtpSettingsForm')
            .find('input[name=SmtpDomain],input[name=SmtpUserName],input[name=SmtpPassword]')
            .closest('.form-group');

        function toggleSmtpCredentialFormGroups() {
            if ($('#Settings_SmtpUseDefaultCredentials').is(':checked')) {
                _$smtpCredentialFormGroups.slideUp('fast');
            } else {
                _$smtpCredentialFormGroups.slideDown('fast');
            }
        }

        $('#Settings_SmtpUseDefaultCredentials').change(function () {
            toggleSmtpCredentialFormGroups();
        });

        toggleSmtpCredentialFormGroups();

        //Toggle LDAP credentials
        var _$ldapCredentialFormGroups = $('#LdapSettingsForm')
            .find('input[name=Domain],input[name=UserName],input[name=Password]')
            .closest('.form-group');

        function toggleLdapCredentialFormGroups() {
            if ($('#Setting_LdapIsEnabled').is(':checked')) {
                _$ldapCredentialFormGroups.slideDown('fast');
            } else {
                _$ldapCredentialFormGroups.slideUp('fast');
            }
        }

        toggleLdapCredentialFormGroups();

        $('#Setting_LdapIsEnabled').change(function () {
            toggleLdapCredentialFormGroups();
        });

        //Save settings
        $('#SaveAllSettingsButton').click(function () {
            _tenantSettingsService.updateAllSettings({
                general: $('#GeneralSettingsForm').serializeFormToObject(),
                userManagement: $.extend($('#FormBasedRegistrationSettingsForm').serializeFormToObject(), $('#OtherSettingsForm').serializeFormToObject()),
                email: $('#EmailSmtpSettingsForm').serializeFormToObject(),
                ldap: $('#LdapSettingsForm').serializeFormToObject()
            }).done(function () {
                abp.notify.info(app.localize('SavedSuccessfully'));
            });
        });
    });
})();