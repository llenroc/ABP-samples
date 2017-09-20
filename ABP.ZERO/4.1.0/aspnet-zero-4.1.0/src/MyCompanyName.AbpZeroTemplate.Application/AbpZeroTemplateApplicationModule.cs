﻿using System.Reflection;
using Abp.AutoMapper;
using Abp.Localization;
using Abp.Modules;
using MyCompanyName.AbpZeroTemplate.Authorization;

namespace MyCompanyName.AbpZeroTemplate
{
    /// <summary>
    /// Application layer module of the application.
    /// </summary>
    [DependsOn(typeof(AbpZeroTemplateCoreModule))]
    public class AbpZeroTemplateApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            //Adding authorization providers
            Configuration.Authorization.Providers.Add<AppAuthorizationProvider>();

            //Adding custom AutoMapper mappings
            Configuration.Modules.AbpAutoMapper().Configurators.Add(mapper =>
            {
                CustomDtoMapper.CreateMappings(mapper);
            });
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}
