using Abp.Runtime.Validation;

namespace MyCompanyName.AbpZeroTemplate.Configuration.Tenants.Dto
{
    public class LdapSettingsEditDto : IValidate
    {
        public bool IsModuleEnabled { get; set; }

        public bool IsEnabled { get; set; }
        
        public string Domain { get; set; }
        
        public string UserName { get; set; }
        
        public string Password { get; set; }
    }
}