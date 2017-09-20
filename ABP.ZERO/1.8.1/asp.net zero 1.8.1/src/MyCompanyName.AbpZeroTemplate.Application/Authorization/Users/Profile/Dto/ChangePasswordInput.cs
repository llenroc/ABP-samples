using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace MyCompanyName.AbpZeroTemplate.Authorization.Users.Profile.Dto
{
    public class ChangePasswordInput : IInputDto
    {
        [Required]
        [StringLength(User.MaxPlainPasswordLength)]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(User.MaxPlainPasswordLength, MinimumLength = User.MinPlainPasswordLength)]
        public string NewPassword { get; set; }
    }
}