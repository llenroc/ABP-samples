using System.Reflection;
using Abp.Modules;
using Abp.Zero.EntityFramework;

namespace MyCompanyName.AbpZeroTemplate
{
    /// <summary>
    /// Entity framework module of the application.
    /// </summary>
    [DependsOn(typeof(AbpZeroEntityFrameworkModule), typeof(AbpZeroTemplateCoreModule))]
    public class AbpZeroTemplateDataModule : AbpModule
    {
        public override void PreInitialize()
        {
            //web.config (or app.config for non-web projects) file should containt a connection string named "Default".
            Configuration.DefaultNameOrConnectionString = "Default";
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}
