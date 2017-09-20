using System.Globalization;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Extensions;
using Abp.Net.Mail;
using Abp.Zero.Configuration;
using MyCompanyName.AbpZeroTemplate.Authorization;
using MyCompanyName.AbpZeroTemplate.Configuration.Host.Dto;

namespace MyCompanyName.AbpZeroTemplate.Configuration.Host
{
    [AbpAuthorize(AppPermissions.Pages_Administration_Host_Settings)]
    public class HostSettingsAppService : AbpZeroTemplateAppServiceBase, IHostSettingsAppService
    {
        public async Task<HostSettingsEditDto> GetAllSettings()
        {
            return new HostSettingsEditDto
                   {
                       General = new GeneralSettingsEditDto
                               {
                                   WebSiteRootAddress = await SettingManager.GetSettingValueAsync(AppSettings.General.WebSiteRootAddress)
                               },
                       UserManagement = new HostUserManagementSettingsEditDto
                               {
                                   IsEmailConfirmationRequiredForLogin = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin)
                               },
                       Email = new EmailSettingsEditDto
                               {
                                   DefaultFromAddress = await SettingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromAddress),
                                   DefaultFromDisplayName = await SettingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromDisplayName),
                                   SmtpHost = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Host),
                                   SmtpPort = await SettingManager.GetSettingValueAsync<int>(EmailSettingNames.Smtp.Port),
                                   SmtpUserName = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.UserName),
                                   SmtpPassword = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Password),
                                   SmtpDomain = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Domain),
                                   SmtpEnableSsl = await SettingManager.GetSettingValueAsync<bool>(EmailSettingNames.Smtp.EnableSsl),
                                   SmtpUseDefaultCredentials = await SettingManager.GetSettingValueAsync<bool>(EmailSettingNames.Smtp.UseDefaultCredentials)
                               }
                   };
        }

        public async Task UpdateAllSettings(HostSettingsEditDto input)
        {
            //General
            await SettingManager.ChangeSettingForApplicationAsync(AppSettings.General.WebSiteRootAddress, input.General.WebSiteRootAddress.EnsureEndsWith('/'));

            //User management
            await SettingManager.ChangeSettingForApplicationAsync(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin, input.UserManagement.IsEmailConfirmationRequiredForLogin.ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture));

            //Email
            await SettingManager.ChangeSettingForApplicationAsync(EmailSettingNames.DefaultFromAddress, input.Email.DefaultFromAddress);
            await SettingManager.ChangeSettingForApplicationAsync(EmailSettingNames.DefaultFromDisplayName, input.Email.DefaultFromDisplayName);
            await SettingManager.ChangeSettingForApplicationAsync(EmailSettingNames.Smtp.Host, input.Email.SmtpHost);
            await SettingManager.ChangeSettingForApplicationAsync(EmailSettingNames.Smtp.Port, input.Email.SmtpPort.ToString(CultureInfo.InvariantCulture));
            await SettingManager.ChangeSettingForApplicationAsync(EmailSettingNames.Smtp.UserName, input.Email.SmtpUserName);
            await SettingManager.ChangeSettingForApplicationAsync(EmailSettingNames.Smtp.Password, input.Email.SmtpPassword);
            await SettingManager.ChangeSettingForApplicationAsync(EmailSettingNames.Smtp.Domain, input.Email.SmtpDomain);
            await SettingManager.ChangeSettingForApplicationAsync(EmailSettingNames.Smtp.EnableSsl, input.Email.SmtpEnableSsl.ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture));
            await SettingManager.ChangeSettingForApplicationAsync(EmailSettingNames.Smtp.UseDefaultCredentials, input.Email.SmtpUseDefaultCredentials.ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture));
        }
    }
}