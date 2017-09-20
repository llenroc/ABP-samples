using System.Reflection;
using Abp.AutoMapper;
using Abp.Modules;
using MyCompanyName.AbpZeroTemplate.Authorization;

namespace MyCompanyName.AbpZeroTemplate
{
    /// <summary>
    /// Application layer module of the application.
    /// </summary>
    [DependsOn(typeof(AbpZeroTemplateCoreModule), typeof(AbpAutoMapperModule))]
    public class AbpZeroTemplateApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            //Adding authorization providers
            Configuration.Authorization.Providers.Add<AppAuthorizationProvider>();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());

            //Custom DTO auto-mappings
            CustomDtoMapper.CreateMappings();
        }
    }
}
