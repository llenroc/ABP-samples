using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Localization;

namespace MyCompanyName.AbpZeroTemplate.Localization.Dto
{
    public class SetDefaultLanguageInput : IInputDto
    {
        [Required]
        [StringLength(ApplicationLanguage.MaxNameLength)]
        public virtual string Name { get; set; }
    }
}