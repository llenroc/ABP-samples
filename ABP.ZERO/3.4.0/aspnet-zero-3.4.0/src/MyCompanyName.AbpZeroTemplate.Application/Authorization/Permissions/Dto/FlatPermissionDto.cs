using Abp.AutoMapper;

namespace MyCompanyName.AbpZeroTemplate.Authorization.Permissions.Dto
{
    [AutoMapFrom(typeof(Abp.Authorization.Permission))]
    public class FlatPermissionDto
    {
        public string ParentName { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public bool IsGrantedByDefault { get; set; }
    }
}