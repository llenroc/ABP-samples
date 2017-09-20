using System.Collections.Generic;
using Abp.Application.Services.Dto;
using MyCompanyName.AbpZeroTemplate.Authorization.Dto;

namespace MyCompanyName.AbpZeroTemplate.Authorization.Users.Dto
{
    public class GetUserPermissionsForEditOutput : IOutputDto
    {
        public List<FlatPermissionDto> Permissions { get; set; }

        public List<string> GrantedPermissionNames { get; set; }
    }
}