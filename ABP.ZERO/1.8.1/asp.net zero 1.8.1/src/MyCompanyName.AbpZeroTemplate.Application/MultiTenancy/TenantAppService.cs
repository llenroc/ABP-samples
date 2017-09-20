using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Notifications;
using MyCompanyName.AbpZeroTemplate.Authorization;
using MyCompanyName.AbpZeroTemplate.Authorization.Roles;
using MyCompanyName.AbpZeroTemplate.Authorization.Users;
using MyCompanyName.AbpZeroTemplate.Editions.Dto;
using MyCompanyName.AbpZeroTemplate.MultiTenancy.Demo;
using MyCompanyName.AbpZeroTemplate.MultiTenancy.Dto;
using MyCompanyName.AbpZeroTemplate.Notifications;

namespace MyCompanyName.AbpZeroTemplate.MultiTenancy
{
    [AbpAuthorize(AppPermissions.Pages_Tenants)]
    public class TenantAppService : AbpZeroTemplateAppServiceBase, ITenantAppService
    {
        private readonly RoleManager _roleManager;
        private readonly IUserEmailer _userEmailer;
        private readonly TenantDemoDataBuilder _demoDataBuilder;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly IAppNotifier _appNotifier;

        public TenantAppService(
            RoleManager roleManager, 
            IUserEmailer userEmailer,
            TenantDemoDataBuilder demoDataBuilder, 
            INotificationSubscriptionManager notificationSubscriptionManager, IAppNotifier appNotifier)
        {
            _roleManager = roleManager;
            _userEmailer = userEmailer;
            _demoDataBuilder = demoDataBuilder;
            _notificationSubscriptionManager = notificationSubscriptionManager;
            _appNotifier = appNotifier;
        }

        public async Task<PagedResultOutput<TenantListDto>> GetTenants(GetTenantsInput input)
        {
            var query = TenantManager.Tenants
                .Include(t => t.Edition)
                .WhereIf(
                    !input.Filter.IsNullOrWhiteSpace(),
                    t =>
                        t.Name.Contains(input.Filter) ||
                        t.TenancyName.Contains(input.Filter)
                );

            var tenantCount = await query.CountAsync();
            var tenants = await query.OrderBy(input.Sorting).PageBy(input).ToListAsync();

            return new PagedResultOutput<TenantListDto>(
                tenantCount,
                tenants.MapTo<List<TenantListDto>>()
                );
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_Create)]
        [UnitOfWork(IsDisabled = true)]
        public async Task CreateTenant(CreateTenantInput input)
        {
            int newTenantId;
            long newAdminId;

            using (var uow = UnitOfWorkManager.Begin())
            {
                //Create tenant
                var tenant = new Tenant(input.TenancyName, input.Name) { IsActive = input.IsActive, EditionId = input.EditionId };
                CheckErrors(await TenantManager.CreateAsync(tenant));
                await CurrentUnitOfWork.SaveChangesAsync(); //To get new tenant's id.

                //We are working entities of new tenant, so changing tenant filter
                using (CurrentUnitOfWork.SetFilterParameter(AbpDataFilters.MayHaveTenant, AbpDataFilters.Parameters.TenantId, tenant.Id))
                {
                    //Create static roles for new tenant
                    CheckErrors(await _roleManager.CreateStaticRoles(tenant.Id));
                    await CurrentUnitOfWork.SaveChangesAsync(); //To get static role ids

                    //grant all permissions to admin role
                    var adminRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.Admin);
                    await _roleManager.GrantAllPermissionsAsync(adminRole);

                    //User role should be default
                    var userRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.User);
                    userRole.IsDefault = true;
                    CheckErrors(await _roleManager.UpdateAsync(userRole));

                    //Create admin user for the tenant
                    if (input.AdminPassword.IsNullOrEmpty())
                    {
                        input.AdminPassword = User.CreateRandomPassword();
                    }

                    var adminUser = User.CreateTenantAdminUser(tenant.Id, input.AdminEmailAddress, input.AdminPassword);
                    adminUser.ShouldChangePasswordOnNextLogin = input.ShouldChangePasswordOnNextLogin;
                    adminUser.IsActive = input.IsActive;

                    CheckErrors(await UserManager.CreateAsync(adminUser));
                    await CurrentUnitOfWork.SaveChangesAsync(); //To get admin user's id

                    //Assign admin user to admin role!
                    CheckErrors(await UserManager.AddToRoleAsync(adminUser.Id, adminRole.Name));

                    //Notifications
                    await _appNotifier.WelcomeToTheApplicationAsync(adminUser);

                    //Send activation email
                    if (input.SendActivationEmail)
                    {
                        adminUser.SetNewEmailConfirmationCode();
                        await _userEmailer.SendEmailActivationLinkAsync(adminUser, input.AdminPassword);
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();

                    await _demoDataBuilder.BuildForAsync(tenant);

                    newTenantId = tenant.Id;
                    newAdminId = adminUser.Id;
                }

                await uow.CompleteAsync();
            }

            //Used a second UOW since UOW above sets some permissions and _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync needs these permissions to be saved.
            using (var uow = UnitOfWorkManager.Begin())
            {
                using (CurrentUnitOfWork.SetFilterParameter(AbpDataFilters.MayHaveTenant, AbpDataFilters.Parameters.TenantId, newTenantId))
                {
                    await _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync(newTenantId, newAdminId);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }

                await uow.CompleteAsync();
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_Edit)]
        public async Task<TenantEditDto> GetTenantForEdit(EntityRequestInput input)
        {
            return (await TenantManager.GetByIdAsync(input.Id)).MapTo<TenantEditDto>();
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_Edit)]
        public async Task UpdateTenant(TenantEditDto input)
        {
            var tenant = await TenantManager.GetByIdAsync(input.Id);
            input.MapTo(tenant);
            CheckErrors(await TenantManager.UpdateAsync(tenant));
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_Delete)]
        public async Task DeleteTenant(EntityRequestInput input)
        {
            var tenant = await TenantManager.GetByIdAsync(input.Id);
            CheckErrors(await TenantManager.DeleteAsync(tenant));
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_ChangeFeatures)]
        public async Task<GetTenantFeaturesForEditOutput> GetTenantFeaturesForEdit(EntityRequestInput input)
        {
            var features = FeatureManager.GetAll();
            var featureValues = await TenantManager.GetFeatureValuesAsync(input.Id);

            return new GetTenantFeaturesForEditOutput
            {
                Features = features.MapTo<List<FlatFeatureDto>>().OrderBy(f => f.DisplayName).ToList(),
                FeatureValues = featureValues.Select(fv => new NameValueDto(fv)).ToList()
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_ChangeFeatures)]
        public async Task UpdateTenantFeatures(UpdateTenantFeaturesInput input)
        {
            await TenantManager.SetFeatureValuesAsync(input.Id, input.FeatureValues.Select(fv => new NameValue(fv.Name, fv.Value)).ToArray());
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_ChangeFeatures)]
        public async Task ResetTenantSpecificFeatures(EntityRequestInput input)
        {
            await TenantManager.ResetAllFeaturesAsync(input.Id);
        }
    }
}