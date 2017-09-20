using Abp.AutoMapper;
using MyCompanyName.AbpZeroTemplate.Web.Authentication.External;

namespace MyCompanyName.AbpZeroTemplate.Web.Models.TokenAuth
{
    [AutoMapFrom(typeof(ExternalLoginProviderInfo))]
    public class ExternalLoginProviderInfoModel
    {
        public string Name { get; set; }

        public string ClientId { get; set; }
    }
}
