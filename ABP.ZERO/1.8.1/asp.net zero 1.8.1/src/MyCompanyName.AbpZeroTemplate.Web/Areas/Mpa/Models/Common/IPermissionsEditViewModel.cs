using System.Collections.Generic;
using MyCompanyName.AbpZeroTemplate.Authorization.Dto;

namespace MyCompanyName.AbpZeroTemplate.Web.Areas.Mpa.Models.Common
{
    public interface IPermissionsEditViewModel
    {
        List<FlatPermissionDto> Permissions { get; set; }

        List<string> GrantedPermissionNames { get; set; }
    }
}