using System.Collections.Generic;

namespace MyCompanyName.AbpZeroTemplate.Web.Authentication.External
{
    public interface IExternalAuthConfiguration
    {
        List<ExternalLoginProviderInfo> Providers { get; }
    }
}