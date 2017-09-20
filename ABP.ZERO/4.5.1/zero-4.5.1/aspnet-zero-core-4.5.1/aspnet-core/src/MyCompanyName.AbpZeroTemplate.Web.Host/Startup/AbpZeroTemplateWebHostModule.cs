using Abp.Configuration.Startup;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Threading.BackgroundWorkers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MyCompanyName.AbpZeroTemplate.Configuration;
using MyCompanyName.AbpZeroTemplate.MultiTenancy;
using MyCompanyName.AbpZeroTemplate.Web.Authentication.External;
using MyCompanyName.AbpZeroTemplate.Web.Authentication.External.Facebook;
using MyCompanyName.AbpZeroTemplate.Web.Authentication.External.Google;
using MyCompanyName.AbpZeroTemplate.Web.Authentication.External.Microsoft;

namespace MyCompanyName.AbpZeroTemplate.Web.Startup
{
    [DependsOn(
        typeof(AbpZeroTemplateWebCoreModule))]
    public class AbpZeroTemplateWebHostModule : AbpModule
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public AbpZeroTemplateWebHostModule(IHostingEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            Configuration.Modules.AbpWebCommon().MultiTenancy.DomainFormat = _appConfiguration["App:ServerRootAddress"] ?? "http://localhost:22742/";
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AbpZeroTemplateWebHostModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            if (IocManager.Resolve<IMultiTenancyConfig>().IsEnabled)
            {
                var workManager = IocManager.Resolve<IBackgroundWorkerManager>();
                workManager.Add(IocManager.Resolve<SubscriptionExpirationCheckWorker>());
                workManager.Add(IocManager.Resolve<SubscriptionExpireEmailNotifierWorker>());
            }

            ConfigureXXX();
        }

        private void ConfigureXXX()
        {
            var externalAuthConfiguration = IocManager.Resolve<ExternalAuthConfiguration>();

            if (bool.Parse(_appConfiguration["Authentication:Facebook:IsEnabled"]))
            {
                externalAuthConfiguration.Providers.Add(
                    new ExternalLoginProviderInfo(
                        FacebookAuthProviderApi.Name,
                        _appConfiguration["Authentication:Facebook:AppId"],
                        _appConfiguration["Authentication:Facebook:AppSecret"],
                        typeof(FacebookAuthProviderApi)
                    )
                );
            }

            if (bool.Parse(_appConfiguration["Authentication:Google:IsEnabled"]))
            {
                externalAuthConfiguration.Providers.Add(
                    new ExternalLoginProviderInfo(
                        GoogleAuthProviderApi.Name,
                        _appConfiguration["Authentication:Google:ClientId"],
                        _appConfiguration["Authentication:Google:ClientSecret"],
                        typeof(GoogleAuthProviderApi)
                    )
                );
            }

            //not implemented yet. Will be implemented with https://github.com/aspnetzero/aspnet-zero-angular/issues/5
            if (bool.Parse(_appConfiguration["Authentication:Microsoft:IsEnabled"]))
            {
                externalAuthConfiguration.Providers.Add(
                    new ExternalLoginProviderInfo(
                        MicrosoftAuthProviderApi.Name,
                        _appConfiguration["Authentication:Microsoft:ConsumerKey"],
                        _appConfiguration["Authentication:Microsoft:ConsumerSecret"],
                        typeof(MicrosoftAuthProviderApi)
                    )
                );
            }
        }
    }
}
