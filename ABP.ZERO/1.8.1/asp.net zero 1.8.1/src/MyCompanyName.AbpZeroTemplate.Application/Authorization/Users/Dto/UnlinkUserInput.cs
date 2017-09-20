using Abp.Application.Services.Dto;

namespace MyCompanyName.AbpZeroTemplate.Authorization.Users.Dto
{
    public class UnlinkUserInput : IInputDto
    {
        public long UserId { get; set; }
    }
}