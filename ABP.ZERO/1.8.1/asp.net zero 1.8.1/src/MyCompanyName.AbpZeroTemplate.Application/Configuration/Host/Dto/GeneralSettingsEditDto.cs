using System.ComponentModel.DataAnnotations;
using Abp.Runtime.Validation;

namespace MyCompanyName.AbpZeroTemplate.Configuration.Host.Dto
{
    public class GeneralSettingsEditDto : IValidate
    {
        [MaxLength(128)]
        public string WebSiteRootAddress { get; set; }
    }
}