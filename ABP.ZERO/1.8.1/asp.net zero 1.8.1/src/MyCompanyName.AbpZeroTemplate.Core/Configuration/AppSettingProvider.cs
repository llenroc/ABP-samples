using System.Collections.Generic;
using System.Configuration;
using Abp.Configuration;

namespace MyCompanyName.AbpZeroTemplate.Configuration
{
    /// <summary>
    /// Defines settings for the application.
    /// See <see cref="AppSettings"/> for setting names.
    /// </summary>
    public class AppSettingProvider : SettingProvider
    {
        public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
        {
            return new[]
                   {
                       //Host settings
                       new SettingDefinition(AppSettings.General.WebSiteRootAddress, "http://localhost:6240/"),

                       //Tenant settings
                       new SettingDefinition(AppSettings.UserManagement.AllowSelfRegistration, ConfigurationManager.AppSettings[AppSettings.UserManagement.UseCaptchaOnRegistration] ?? "true", scopes: SettingScopes.Tenant),
                       new SettingDefinition(AppSettings.UserManagement.IsNewRegisteredUserActiveByDefault, ConfigurationManager.AppSettings[AppSettings.UserManagement.IsNewRegisteredUserActiveByDefault] ?? "false", scopes: SettingScopes.Tenant),
                       new SettingDefinition(AppSettings.UserManagement.UseCaptchaOnRegistration, ConfigurationManager.AppSettings[AppSettings.UserManagement.UseCaptchaOnRegistration] ?? "true", scopes: SettingScopes.Tenant),
                   };
        }
    }
}
