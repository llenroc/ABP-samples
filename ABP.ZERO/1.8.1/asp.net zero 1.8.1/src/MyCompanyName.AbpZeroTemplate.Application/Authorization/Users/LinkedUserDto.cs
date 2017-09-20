using System;
using Abp.Application.Services.Dto;

namespace MyCompanyName.AbpZeroTemplate.Authorization.Users
{
    public class LinkedUserDto : EntityDto<long>
    {
        public string TenancyName { get; set; }

        public string Username { get; set; }

        public Guid? ProfilePictureId { get; set; }

        public object GetShownLoginName(bool multiTenancyEnabled)
        {
            if (!multiTenancyEnabled)
            {
                return Username;
            }

            return string.IsNullOrEmpty(TenancyName)
                ? ".\\" + Username
                : TenancyName + "\\" + Username;
        }
    }
}