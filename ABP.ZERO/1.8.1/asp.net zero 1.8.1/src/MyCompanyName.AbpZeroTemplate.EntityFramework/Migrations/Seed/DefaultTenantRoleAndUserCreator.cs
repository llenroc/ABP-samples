using System.Linq;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using MyCompanyName.AbpZeroTemplate.Authorization;
using MyCompanyName.AbpZeroTemplate.Authorization.Roles;
using MyCompanyName.AbpZeroTemplate.Authorization.Users;
using MyCompanyName.AbpZeroTemplate.Editions;
using MyCompanyName.AbpZeroTemplate.EntityFramework;
using MyCompanyName.AbpZeroTemplate.MultiTenancy;

namespace MyCompanyName.AbpZeroTemplate.Migrations.Seed
{
    /// <summary>
    /// This class is used to default tenants, roles and users when application is installed.
    /// It's also used in tests as startup state of the system.
    /// </summary>
    public class DefaultTenantRoleAndUserCreator
    {
        private readonly AbpZeroTemplateDbContext _context;

        public DefaultTenantRoleAndUserCreator(AbpZeroTemplateDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            CreateHostAndUsers();
            CreateDefaultTenantAndUsers();
        }

        private void CreateHostAndUsers()
        {
            //Admin role for host

            var adminRoleForHost = _context.Roles.FirstOrDefault(r => r.TenantId == null && r.Name == StaticRoleNames.Host.Admin);
            if (adminRoleForHost == null)
            {
                adminRoleForHost = _context.Roles.Add(new Role(null, StaticRoleNames.Host.Admin, StaticRoleNames.Host.Admin) { IsStatic = true, IsDefault = true });
                _context.SaveChanges();
            }

            //admin user for host

            var adminUserForHost = _context.Users.FirstOrDefault(u => u.TenantId == null && u.UserName == User.AdminUserName);
            if (adminUserForHost == null)
            {
                adminUserForHost = _context.Users.Add(
                    new User
                    {
                        TenantId = null,
                        UserName = User.AdminUserName,
                        Name = "admin",
                        Surname = "admin",
                        EmailAddress = "admin@aspnetzero.com",
                        IsEmailConfirmed = true,
                        ShouldChangePasswordOnNextLogin = true,
                        IsActive = true,
                        Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" //123qwe
                    });
                _context.SaveChanges();

                //Assign Admin role to admin user
                _context.UserRoles.Add(new UserRole(adminUserForHost.Id, adminRoleForHost.Id));
                _context.SaveChanges();

                //Grant all permissions
                var permissions = PermissionFinder
                    .GetAllPermissions(new AppAuthorizationProvider())
                    .Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Host))
                    .ToList();

                foreach (var permission in permissions)
                {
                    if (!permission.IsGrantedByDefault)
                    {
                        _context.Permissions.Add(
                            new RolePermissionSetting
                            {
                                Name = permission.Name,
                                IsGranted = true,
                                RoleId = adminRoleForHost.Id
                            });
                    }
                }

                _context.SaveChanges();
            }
        }

        private void CreateDefaultTenantAndUsers()
        {
            //Default tenant

            var defaultTenant = _context.Tenants.FirstOrDefault(t => t.TenancyName == Tenant.DefaultTenantName);
            if (defaultTenant == null)
            {
                defaultTenant = new Tenant(Tenant.DefaultTenantName, Tenant.DefaultTenantName);

                var defaultEdition = _context.Editions.FirstOrDefault(e => e.Name == EditionManager.DefaultEditionName);
                if (defaultEdition != null)
                {
                    defaultTenant.EditionId = defaultEdition.Id;
                }

                defaultTenant = _context.Tenants.Add(defaultTenant);
                _context.SaveChanges();
            }

            //Admin role for 'Default' tenant

            var adminRoleForDefaultTenant = _context.Roles.FirstOrDefault(r => r.TenantId == defaultTenant.Id && r.Name == StaticRoleNames.Tenants.Admin);
            if (adminRoleForDefaultTenant == null)
            {
                adminRoleForDefaultTenant = _context.Roles.Add(new Role(defaultTenant.Id, StaticRoleNames.Tenants.Admin, StaticRoleNames.Tenants.Admin) { IsStatic = true });
                _context.SaveChanges();
            }

            //User role for 'Default' tenant

            var userRoleForDefaultTenant = _context.Roles.FirstOrDefault(r => r.TenantId == defaultTenant.Id && r.Name == StaticRoleNames.Tenants.User);
            if (userRoleForDefaultTenant == null)
            {
                _context.Roles.Add(new Role(defaultTenant.Id, StaticRoleNames.Tenants.User, StaticRoleNames.Tenants.User) { IsStatic = true, IsDefault = true });
                _context.SaveChanges();
            }

            //admin user for 'Default' tenant

            var adminUserForDefaultTenant = _context.Users.FirstOrDefault(u => u.TenantId == defaultTenant.Id && u.UserName == User.AdminUserName);
            if (adminUserForDefaultTenant == null)
            {
                adminUserForDefaultTenant = User.CreateTenantAdminUser(defaultTenant.Id, "admin@defaulttenant.com", "123qwe");
                adminUserForDefaultTenant.IsEmailConfirmed = true;
                adminUserForDefaultTenant.ShouldChangePasswordOnNextLogin = true;
                adminUserForDefaultTenant.IsActive = true;

                _context.Users.Add(adminUserForDefaultTenant);
                _context.SaveChanges();

                //Assign Admin role to admin user
                _context.UserRoles.Add(new UserRole(adminUserForDefaultTenant.Id, adminRoleForDefaultTenant.Id));
                _context.SaveChanges();

                //Grant all permissions
                var permissions = PermissionFinder
                    .GetAllPermissions(new AppAuthorizationProvider())
                    .Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Tenant))
                    .ToList();

                foreach (var permission in permissions)
                {
                    if (!permission.IsGrantedByDefault)
                    {
                        _context.Permissions.Add(
                            new RolePermissionSetting
                            {
                                Name = permission.Name,
                                IsGranted = true,
                                RoleId = adminRoleForDefaultTenant.Id
                            });
                    }
                }

                _context.SaveChanges();
            }
        }
    }
}
