using System;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Abp.Collections;
using Abp.Configuration.Startup;
using Abp.Modules;
using Abp.Runtime.Session;
using Abp.TestBase;
using Castle.MicroKernel.Registration;
using Effort;
using EntityFramework.DynamicFilters;
using MyCompanyName.AbpZeroTemplate.Authorization.Roles;
using MyCompanyName.AbpZeroTemplate.Authorization.Users;
using MyCompanyName.AbpZeroTemplate.EntityFramework;
using MyCompanyName.AbpZeroTemplate.Migrations.Seed;
using MyCompanyName.AbpZeroTemplate.MultiTenancy;
using MyCompanyName.AbpZeroTemplate.Tests.TestDatas;

namespace MyCompanyName.AbpZeroTemplate.Tests
{
    /// <summary>
    /// This is base class for all our test classes.
    /// It prepares ABP system, modules and a fake, in-memory database.
    /// Seeds database with initial data.
    /// Provides methods to easily work with <see cref="AbpZeroTemplateDbContext"/>.
    /// </summary>
    public abstract class AppTestBase : AbpIntegratedTestBase
    {
        protected AppTestBase()
        {
            //Seed initial data
            UsingDbContext(context =>
            {
                new InitialDbBuilder(context).Create();
                new TestDataBuilder(context).Create();
            });

            LoginAsDefaultTenantAdmin();
        }

        protected override void PreInitialize()
        {
            base.PreInitialize();

            //Fake DbConnection using Effort!
            LocalIocManager.IocContainer.Register(
                Component.For<DbConnection>()
                    .UsingFactoryMethod(DbConnectionFactory.CreateTransient)
                    .LifestyleSingleton()
                );
        }

        protected override void AddModules(ITypeList<AbpModule> modules)
        {
            base.AddModules(modules);

            //Adding testing modules. Depended modules of these modules are automatically added.
            modules.Add<AbpZeroTemplateTestModule>();
        }

        #region UsingDbContext

        protected void UsingDbContext(Action<AbpZeroTemplateDbContext> action)
        {
            using (var context = LocalIocManager.Resolve<AbpZeroTemplateDbContext>())
            {
                context.DisableAllFilters();
                action(context);
                context.SaveChanges();
            }
        }

        protected async Task UsingDbContextAsync(Action<AbpZeroTemplateDbContext> action)
        {
            using (var context = LocalIocManager.Resolve<AbpZeroTemplateDbContext>())
            {
                context.DisableAllFilters();
                action(context);
                await context.SaveChangesAsync();
            }
        }

        protected T UsingDbContext<T>(Func<AbpZeroTemplateDbContext, T> func)
        {
            T result;

            using (var context = LocalIocManager.Resolve<AbpZeroTemplateDbContext>())
            {
                context.DisableAllFilters();
                result = func(context);
                context.SaveChanges();
            }

            return result;
        }

        protected async Task<T> UsingDbContextAsync<T>(Func<AbpZeroTemplateDbContext, Task<T>> func)
        {
            T result;

            using (var context = LocalIocManager.Resolve<AbpZeroTemplateDbContext>())
            {
                context.DisableAllFilters();
                result = await func(context);
                await context.SaveChangesAsync();
            }

            return result;
        }

        #endregion

        #region Login

        protected void LoginAsHostAdmin()
        {
            LoginAsHost(User.AdminUserName);
        }

        protected void LoginAsDefaultTenantAdmin()
        {
            LoginAsTenant(Tenant.DefaultTenantName, User.AdminUserName);
        }

        protected void LoginAsHost(string userName)
        {
            Resolve<IMultiTenancyConfig>().IsEnabled = true;

            AbpSession.TenantId = null;

            var user = UsingDbContext(context => context.Users.FirstOrDefault(u => u.TenantId == AbpSession.TenantId && u.UserName == userName));
            if (user == null)
            {
                throw new Exception("There is no user: " + userName + " for host.");
            }

            AbpSession.UserId = user.Id;
        }

        protected void LoginAsTenant(string tenancyName, string userName)
        {
            var tenant = UsingDbContext(context => context.Tenants.FirstOrDefault(t => t.TenancyName == tenancyName));
            if (tenant == null)
            {
                throw new Exception("There is no tenant: " + tenancyName);
            }

            AbpSession.TenantId = tenant.Id;

            var user = UsingDbContext(context => context.Users.FirstOrDefault(u => u.TenantId == AbpSession.TenantId && u.UserName == userName));
            if (user == null)
            {
                throw new Exception("There is no user: " + userName + " for tenant: " + tenancyName);
            }

            AbpSession.UserId = user.Id;
        }

        #endregion

        #region GetCurrentUser

        /// <summary>
        /// Gets current user if <see cref="IAbpSession.UserId"/> is not null.
        /// Throws exception if it's null.
        /// </summary>
        protected User GetCurrentUser()
        {
            var userId = AbpSession.GetUserId();
            return UsingDbContext(context => context.Users.Single(u => u.Id == userId));
        }

        /// <summary>
        /// Gets current user if <see cref="IAbpSession.UserId"/> is not null.
        /// Throws exception if it's null.
        /// </summary>
        protected async Task<User> GetCurrentUserAsync()
        {
            var userId = AbpSession.GetUserId();
            return await UsingDbContext(context => context.Users.SingleAsync(u => u.Id == userId));
        }

        #endregion

        #region GetCurrentTenant

        /// <summary>
        /// Gets current tenant if <see cref="IAbpSession.TenantId"/> is not null.
        /// Throws exception if there is no current tenant.
        /// </summary>
        protected Tenant GetCurrentTenant()
        {
            var tenantId = AbpSession.GetTenantId();
            return UsingDbContext(context => context.Tenants.Single(t => t.Id == tenantId));
        }

        /// <summary>
        /// Gets current tenant if <see cref="IAbpSession.TenantId"/> is not null.
        /// Throws exception if there is no current tenant.
        /// </summary>
        protected async Task<Tenant> GetCurrentTenantAsync()
        {
            var tenantId = AbpSession.GetTenantId();
            return await UsingDbContext(context => context.Tenants.SingleAsync(t => t.Id == tenantId));
        }

        #endregion

        #region GetTenant / GetTenantOrNull

        protected Tenant GetTenant(string tenancyName)
        {
            return UsingDbContext(context => context.Tenants.Single(t => t.TenancyName == tenancyName));
        }

        protected async Task<Tenant> GetTenantAsync(string tenancyName)
        {
            return await UsingDbContext(async context => await context.Tenants.SingleAsync(t => t.TenancyName == tenancyName));
        }

        protected Tenant GetTenantOrNull(string tenancyName)
        {
            return UsingDbContext(context => context.Tenants.FirstOrDefault(t => t.TenancyName == tenancyName));
        }

        protected async Task<Tenant> GetTenantOrNullAsync(string tenancyName)
        {
            return await UsingDbContext(async context => await context.Tenants.FirstOrDefaultAsync(t => t.TenancyName == tenancyName));
        }

        #endregion

        #region GetRole

        protected Role GetRole(string roleName)
        {
            return UsingDbContext(context => context.Roles.Single(r => r.Name == roleName && r.TenantId == AbpSession.TenantId));
        }

        protected async Task<Role> GetRoleAsync(string roleName)
        {
            return await UsingDbContext(async context => await context.Roles.SingleAsync(r => r.Name == roleName && r.TenantId == AbpSession.TenantId));
        }

        #endregion

        #region GetUserByUserName

        protected User GetUserByUserName(string userName)
        {
            var user = GetUserByUserNameOrNull(userName);
            if (user == null)
            {
                throw new ApplicationException("Can not find a user with username: " + userName);
            }

            return user;
        }

        protected async Task<User> GetUserByUserNameAsync(string userName)
        {
            var user = await GetUserByUserNameOrNullAsync(userName);
            if (user == null)
            {
                throw new ApplicationException("Can not find a user with username: " + userName);
            }

            return user;
        }

        protected User GetUserByUserNameOrNull(string userName)
        {
            return UsingDbContext(context =>
                context.Users.FirstOrDefault(u =>
                    u.UserName == userName &&
                    u.TenantId == AbpSession.TenantId
                    ));
        }

        protected async Task<User> GetUserByUserNameOrNullAsync(string userName)
        {
            return await UsingDbContext(async context =>
                await context.Users.FirstOrDefaultAsync(u =>
                    u.UserName == userName &&
                    u.TenantId == AbpSession.TenantId
                    ));
        }

        #endregion
    }
}
