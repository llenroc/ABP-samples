using Abp.Dependency;
using Abp.MultiTenancy;
using Microsoft.AspNetCore.Hosting;
using MyCompanyName.AbpZeroTemplate.Url;

namespace MyCompanyName.AbpZeroTemplate.Web.Url
{
    public class WebUrlService : WebUrlServiceBase, IWebUrlService, ITransientDependency
    {
        public WebUrlService(
            IHostingEnvironment hostingEnvironment,
            ITenantCache tenantCache) :
            base(hostingEnvironment, tenantCache)
        {
        }

        public override string WebSiteRootAddressFormatKey => "App:ClientRootAddress";

        public override string ServerRootAddressFormatKey => "App:ServerRootAddress";
    }
}