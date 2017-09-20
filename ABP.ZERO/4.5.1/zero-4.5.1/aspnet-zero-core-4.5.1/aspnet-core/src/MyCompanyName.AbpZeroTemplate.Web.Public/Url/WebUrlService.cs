using Abp.Dependency;
using Abp.MultiTenancy;
using Microsoft.AspNetCore.Hosting;
using MyCompanyName.AbpZeroTemplate.Url;
using MyCompanyName.AbpZeroTemplate.Web.Url;

namespace MyCompanyName.AbpZeroTemplate.Web.Public.Url
{
    public class WebUrlService : WebUrlServiceBase, IWebUrlService, ITransientDependency
    {
        public WebUrlService(
            IHostingEnvironment hostingEnvironment,
            ITenantCache tenantCache) :
            base(hostingEnvironment, tenantCache)
        {
        }

        public override string WebSiteRootAddressFormatKey => "App:WebSiteRootAddress";

        public override string ServerRootAddressFormatKey => "App:AdminWebSiteRootAddress";
    }
}