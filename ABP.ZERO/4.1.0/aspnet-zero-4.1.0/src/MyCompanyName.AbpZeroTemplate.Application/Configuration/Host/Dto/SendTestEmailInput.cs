using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using MyCompanyName.AbpZeroTemplate.Authorization.Users;

namespace MyCompanyName.AbpZeroTemplate.Configuration.Host.Dto
{
    public class SendTestEmailInput
    {
        [Required]
        [MaxLength(User.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }
    }
}