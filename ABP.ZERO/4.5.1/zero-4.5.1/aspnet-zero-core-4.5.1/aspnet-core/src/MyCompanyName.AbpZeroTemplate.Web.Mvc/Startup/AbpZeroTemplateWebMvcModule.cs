using Abp.Configuration.Startup;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Threading.BackgroundWorkers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MyCompanyName.AbpZeroTemplate.Configuration;
using MyCompanyName.AbpZeroTemplate.MultiTenancy;
using MyCompanyName.AbpZeroTemplate.Web.Areas.AppAreaName.Startup;

namespace MyCompanyName.AbpZeroTemplate.Web.Startup
{
    [DependsOn(
        typeof(AbpZeroTemplateWebCoreModule)
    )]
    public class AbpZeroTemplateWebMvcModule : AbpModule
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public AbpZeroTemplateWebMvcModule(IHostingEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            Configuration.Modules.AbpWebCommon().MultiTenancy.DomainFormat = _appConfiguration["App:WebSiteRootAddress"] ?? "http://localhost:62114/";

            Configuration.Navigation.Providers.Add<AppAreaNameNavigationProvider>();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AbpZeroTemplateWebMvcModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            if (!IocManager.Resolve<IMultiTenancyConfig>().IsEnabled)
            {
                return;
            }
            var workManager = IocManager.Resolve<IBackgroundWorkerManager>();
            workManager.Add(IocManager.Resolve<SubscriptionExpirationCheckWorker>());
            workManager.Add(IocManager.Resolve<SubscriptionExpireEmailNotifierWorker>());
        }
    }
}