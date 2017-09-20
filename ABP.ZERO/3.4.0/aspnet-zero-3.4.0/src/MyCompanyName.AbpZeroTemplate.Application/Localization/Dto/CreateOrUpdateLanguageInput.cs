using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace MyCompanyName.AbpZeroTemplate.Localization.Dto
{
    public class CreateOrUpdateLanguageInput
    {
        [Required]
        public ApplicationLanguageEditDto Language { get; set; }
    }
}