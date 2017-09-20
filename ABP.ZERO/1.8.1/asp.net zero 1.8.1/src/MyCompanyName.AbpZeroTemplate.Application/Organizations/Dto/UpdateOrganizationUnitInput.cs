using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Organizations;

namespace MyCompanyName.AbpZeroTemplate.Organizations.Dto
{
    public class UpdateOrganizationUnitInput : IInputDto
    {
        [Range(1, long.MaxValue)]
        public long Id { get; set; }

        [Required]
        [StringLength(OrganizationUnit.MaxDisplayNameLength)]
        public string DisplayName { get; set; }
    }
}