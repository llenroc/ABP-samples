using System.Data.Entity;
using System.Threading.Tasks;
using MyCompanyName.AbpZeroTemplate.Authorization.Users;
using MyCompanyName.AbpZeroTemplate.Authorization.Users.Dto;
using MyCompanyName.AbpZeroTemplate.MultiTenancy;
using Shouldly;
using Xunit;

namespace MyCompanyName.AbpZeroTemplate.Tests.Authorization.Users
{
    public class UserAppService_Link_Tests : UserAppServiceTestBase
    {
        private readonly IUserLinkAppService _userLinkAppService;

        public UserAppService_Link_Tests()
        {
            _userLinkAppService = Resolve<IUserLinkAppService>();
        }

        [Fact]
        public async Task Should_Link_User_To_Host_Admin()
        {
            LoginAsHostAdmin();
            await LinkUserAndTestAsync(string.Empty);
        }

        [Fact]
        public async Task Should_Link_User_To_Default_Tenant_Admin()
        {
            LoginAsDefaultTenantAdmin();
            await LinkUserAndTestAsync(Tenant.DefaultTenantName);
        }

        [Fact]
        public async Task Should_Link_User_To_Different_Tenant_User()
        {
            //Arrange
            LoginAsDefaultTenantAdmin();
            await CreateTestTenantAndTestUser();

            //Act
            await _userLinkAppService.LinkToUser(new LinkToUserInput
            {
                TenancyName = "Test",
                UsernameOrEmailAddress = "test",
                Password = "123qwe"
            });

            //Assert
            await UsingDbContextAsync(async context =>
            {
                var linkedUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "test");
                var currentUser = await context.Users.FirstOrDefaultAsync(u => u.Id == AbpSession.UserId);

                currentUser.UserLinkId.HasValue.ShouldBe(true);
                currentUser.UserLinkId.Value.ShouldBe(currentUser.Id);

                linkedUser.UserLinkId.HasValue.ShouldBe(true);
                linkedUser.UserLinkId.Value.ShouldBe(currentUser.Id);
            });
        }

        [Fact]
        public async Task Should_Link_User_To_Already_Linked_User()
        {
            //Arrange
            LoginAsHostAdmin();
            await CreateTestTenantAndTestUser();

            LoginAsDefaultTenantAdmin();
            CreateTestUsers();

            var linkToTestTenantUserInput = new LinkToUserInput
            {
                TenancyName = "Test",
                UsernameOrEmailAddress = "test",
                Password = "123qwe"
            };

            //Act
            //Link Default\admin -> Test\test
            await _userLinkAppService.LinkToUser(linkToTestTenantUserInput);

            LoginAsTenant(Tenant.DefaultTenantName, "jnash");
            //Link Default\jnash->Test\test
            await _userLinkAppService.LinkToUser(linkToTestTenantUserInput);

            //Assert
            await UsingDbContextAsync(async context =>
            {
                var defaultTenantAdmin = await context.Users.FirstOrDefaultAsync(u => u.Id == AbpSession.UserId);
                var jnash = await context.Users.FirstOrDefaultAsync(u => u.UserName == "jnash");
                var testTenantUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "test");

                jnash.UserLinkId.ShouldBe(jnash.Id);
                defaultTenantAdmin.UserLinkId.ShouldBe(jnash.Id);
                testTenantUser.UserLinkId.ShouldBe(jnash.Id);
            });
        }

        private async Task CreateTestTenantAndTestUser()
        {
            await UsingDbContextAsync(async context =>
            {
                var testTenant = new Tenant("Test", "test");
                context.Tenants.Add(testTenant);
                await context.SaveChangesAsync();

                context.Users.Add(new User
                {
                    EmailAddress = "test@test.com",
                    IsEmailConfirmed = true,
                    Name = "Test",
                    Surname = "User",
                    UserName = "test",
                    Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==", //123qwe
                    TenantId = testTenant.Id
                });

                await context.SaveChangesAsync();
            });
        }

        private async Task LinkUserAndTestAsync(string tenancyName)
        {
            //Arrange
            CreateTestUsers();

            //Act
            await _userLinkAppService.LinkToUser(new LinkToUserInput
            {
                TenancyName = tenancyName,
                UsernameOrEmailAddress = "jnash",
                Password = "123qwe"
            });

            //Assert
            await UsingDbContextAsync(async context =>
            {
                var linkedUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "jnash");
                var currentUser = await context.Users.FirstOrDefaultAsync(u => u.Id == AbpSession.UserId);

                currentUser.UserLinkId.HasValue.ShouldBe(true);
                currentUser.UserLinkId.Value.ShouldBe(currentUser.Id);

                linkedUser.UserLinkId.HasValue.ShouldBe(true);
                linkedUser.UserLinkId.Value.ShouldBe(currentUser.Id);
            });
        }
    }
}
