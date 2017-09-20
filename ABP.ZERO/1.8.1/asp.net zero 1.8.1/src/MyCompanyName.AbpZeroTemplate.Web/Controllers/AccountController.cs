using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Notifications;
using Abp.Runtime.Caching;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Abp.Threading;
using Abp.Timing;
using Abp.UI;
using Abp.Web.Mvc.Authorization;
using Abp.Web.Mvc.Models;
using Abp.Zero.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MyCompanyName.AbpZeroTemplate.Authorization;
using MyCompanyName.AbpZeroTemplate.Authorization.Impersonation;
using MyCompanyName.AbpZeroTemplate.Authorization.Roles;
using MyCompanyName.AbpZeroTemplate.Authorization.Users;
using MyCompanyName.AbpZeroTemplate.Configuration;
using MyCompanyName.AbpZeroTemplate.Debugging;
using MyCompanyName.AbpZeroTemplate.MultiTenancy;
using MyCompanyName.AbpZeroTemplate.Notifications;
using MyCompanyName.AbpZeroTemplate.Security;
using MyCompanyName.AbpZeroTemplate.Web.Controllers.Results;
using MyCompanyName.AbpZeroTemplate.Web.Models.Account;
using MyCompanyName.AbpZeroTemplate.Web.MultiTenancy;
using Recaptcha.Web;
using Recaptcha.Web.Mvc;

namespace MyCompanyName.AbpZeroTemplate.Web.Controllers
{
    public class AccountController : AbpZeroTemplateControllerBase
    {
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly TenantManager _tenantManager;
        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly IUserEmailer _userEmailer;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ITenancyNameFinder _tenancyNameFinder;
        private readonly ICacheManager _cacheManager;
        private readonly IWebUrlService _webUrlService;
        private readonly IAppNotifier _appNotifier;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly IUserLinkManager _userLinkManager;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public AccountController(
            UserManager userManager,
            IMultiTenancyConfig multiTenancyConfig,
            IUserEmailer userEmailer,
            RoleManager roleManager,
            TenantManager tenantManager,
            IUnitOfWorkManager unitOfWorkManager,
            ITenancyNameFinder tenancyNameFinder, 
            ICacheManager cacheManager, 
            IAppNotifier appNotifier,
            IWebUrlService webUrlService,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            IUserLinkManager userLinkManager, 
            INotificationSubscriptionManager notificationSubscriptionManager)
        {
            _userManager = userManager;
            _multiTenancyConfig = multiTenancyConfig;
            _userEmailer = userEmailer;
            _roleManager = roleManager;
            _tenantManager = tenantManager;
            _unitOfWorkManager = unitOfWorkManager;
            _tenancyNameFinder = tenancyNameFinder;
            _cacheManager = cacheManager;
            _webUrlService = webUrlService;
            _appNotifier = appNotifier;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _userLinkManager = userLinkManager;
            _notificationSubscriptionManager = notificationSubscriptionManager;
        }

        #region Login / Logout
        // get 方法获取一个view
        public ActionResult Login(string userNameOrEmailAddress = "", string returnUrl = "", string successMessage = "")
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = Url.Action("Index", "Application");
            }

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled;

            return View(
                new LoginFormViewModel
                {
                    TenancyName = _tenancyNameFinder.GetCurrentTenancyNameOrNull(),
                    IsSelfRegistrationEnabled = IsSelfRegistrationEnabled(),
                    SuccessMessage = successMessage,
                    UserNameOrEmailAddress = userNameOrEmailAddress
                });
        }

        [HttpPost]
        [UnitOfWork]
        [DisableAuditing]
        public virtual async Task<JsonResult> Login(LoginViewModel loginModel, string returnUrl = "", string returnUrlHash = "")
        {
            CheckModelState();

            _unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant);

            var loginResult = await GetLoginResultAsync(loginModel.UsernameOrEmailAddress, loginModel.Password, loginModel.TenancyName);

            if (loginResult.User.ShouldChangePasswordOnNextLogin)
            {
                loginResult.User.SetNewPasswordResetCode();

                return Json(new MvcAjaxResponse
                {
                    TargetUrl = Url.Action(
                        "ResetPassword",
                        new ResetPasswordViewModel
                        {
                            UserId = SimpleStringCipher.Encrypt(loginResult.User.Id.ToString()),
                            ResetCode = loginResult.User.PasswordResetCode
                        })
                });
            }

            await SignInAsync(loginResult.User, loginResult.Identity, loginModel.RememberMe);

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = Url.Action("Index", "Application");
            }

            if (!string.IsNullOrWhiteSpace(returnUrlHash))
            {
                returnUrl = returnUrl + returnUrlHash;
            }

            return Json(new MvcAjaxResponse { TargetUrl = returnUrl });
        }

        public ActionResult Logout()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Login");
        }

        private async Task SignInAsync(User user, ClaimsIdentity identity = null, bool rememberMe = false)
        {
            if (identity == null)
            {
                identity = await _userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            }

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = rememberMe }, identity);
        }

        private async Task<AbpUserManager<Tenant, Role, User>.AbpLoginResult> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
        {
            var loginResult = await _userManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
            }
        }

        #endregion

        #region Register

        public ActionResult Register()
        {
            return RegisterView(new RegisterViewModel
            {
                TenancyName = _tenancyNameFinder.GetCurrentTenancyNameOrNull()
            });
        }

        public ActionResult RegisterView(RegisterViewModel model)
        {
            CheckSelfRegistrationIsEnabled();

            ViewBag.IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled;
            ViewBag.UseCaptcha = !model.IsExternalLogin && UseCaptchaOnRegistration();

            return View("Register", model);
        }

        [HttpPost]
        [UnitOfWork]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Register(RegisterViewModel model)
        {
            try
            {
                CheckSelfRegistrationIsEnabled();

                CheckModelState();

                if (!model.IsExternalLogin && UseCaptchaOnRegistration())
                {
                    var recaptchaHelper = this.GetRecaptchaVerificationHelper();
                    if (recaptchaHelper.Response.IsNullOrEmpty())
                    {
                        throw new UserFriendlyException(L("CaptchaCanNotBeEmpty"));
                    }

                    if (recaptchaHelper.VerifyRecaptchaResponse() != RecaptchaVerificationResult.Success)
                    {
                        throw new UserFriendlyException(L("IncorrectCaptchaAnswer"));
                    }
                }

                if (!_multiTenancyConfig.IsEnabled)
                {
                    model.TenancyName = Tenant.DefaultTenantName;
                }
                else if (model.TenancyName.IsNullOrEmpty())
                {
                    throw new UserFriendlyException(L("TenantNameCanNotBeEmpty"));
                }

                var tenant = await GetActiveTenantAsync(model.TenancyName);

                if (!await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.UserManagement.AllowSelfRegistration, tenant.Id))
                {
                    throw new UserFriendlyException(L("SelfUserRegistrationIsDisabledMessage_Detail"));
                }

                //Getting tenant-specific settings
                var isNewRegisteredUserActiveByDefault = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.UserManagement.IsNewRegisteredUserActiveByDefault, tenant.Id);
                var isEmailConfirmationRequiredForLogin = await SettingManager.GetSettingValueForTenantAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin, tenant.Id);

                var user = new User
                {
                    TenantId = tenant.Id,
                    Name = model.Name,
                    Surname = model.Surname,
                    EmailAddress = model.EmailAddress,
                    IsActive = isNewRegisteredUserActiveByDefault
                };

                ExternalLoginInfo externalLoginInfo = null;
                if (model.IsExternalLogin)
                {
                    externalLoginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
                    if (externalLoginInfo == null)
                    {
                        throw new ApplicationException("Can not external login!");
                    }

                    user.Logins = new List<UserLogin>
                    {
                        new UserLogin
                        {
                            LoginProvider = externalLoginInfo.Login.LoginProvider,
                            ProviderKey = externalLoginInfo.Login.ProviderKey
                        }
                    };

                    model.UserName = model.EmailAddress;
                    model.Password = Authorization.Users.User.CreateRandomPassword();

                    if (string.Equals(externalLoginInfo.Email, model.EmailAddress, StringComparison.InvariantCultureIgnoreCase))
                    {
                        user.IsEmailConfirmed = true;                        
                    }
                }
                else
                {
                    if (model.UserName.IsNullOrEmpty() || model.Password.IsNullOrEmpty())
                    {
                        throw new UserFriendlyException(L("FormIsNotValidMessage"));                        
                    }
                }

                user.UserName = model.UserName;
                user.Password = new PasswordHasher().HashPassword(model.Password);

                _unitOfWorkManager.Current.EnableFilter(AbpDataFilters.MayHaveTenant);
                _unitOfWorkManager.Current.SetFilterParameter(AbpDataFilters.MayHaveTenant, AbpDataFilters.Parameters.TenantId, tenant.Id);

                user.Roles = new List<UserRole>();
                foreach (var defaultRole in await _roleManager.Roles.Where(r => r.IsDefault).ToListAsync())
                {
                    user.Roles.Add(new UserRole { RoleId = defaultRole.Id });
                }

                CheckErrors(await _userManager.CreateAsync(user));
                await _unitOfWorkManager.Current.SaveChangesAsync();

                if (!user.IsEmailConfirmed)
                {
                    user.SetNewEmailConfirmationCode();
                    await _userEmailer.SendEmailActivationLinkAsync(user);
                }

                //Notifications
                await _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync(user.TenantId, user.Id);
                await _appNotifier.WelcomeToTheApplicationAsync(user);
                await _appNotifier.NewUserRegisteredAsync(user);

                //Directly login if possible
                if (user.IsActive && (user.IsEmailConfirmed || !isEmailConfirmationRequiredForLogin))
                {
                    AbpUserManager<Tenant, Role, User>.AbpLoginResult loginResult;
                    if (externalLoginInfo != null)
                    {
                        loginResult = await _userManager.LoginAsync(externalLoginInfo.Login, tenant.TenancyName);
                    }
                    else
                    {
                        loginResult = await GetLoginResultAsync(user.UserName, model.Password, tenant.TenancyName);
                    }

                    if (loginResult.Result == AbpLoginResultType.Success)
                    {
                        await SignInAsync(loginResult.User, loginResult.Identity);
                        return Redirect(Url.Action("Index", "Application"));
                    }

                    Logger.Warn("New registered user could not be login. This should not be normally. login result: " + loginResult.Result);
                }

                return View("RegisterResult", new RegisterResultViewModel
                {
                    TenancyName = tenant.TenancyName,
                    NameAndSurname = user.Name + " " + user.Surname,
                    UserName = user.UserName,
                    EmailAddress = user.EmailAddress,
                    IsActive = user.IsActive,
                    IsEmailConfirmationRequired = isEmailConfirmationRequiredForLogin
                });
            }
            catch (UserFriendlyException ex)
            {
                ViewBag.IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled;
                ViewBag.UseCaptcha = !model.IsExternalLogin && UseCaptchaOnRegistration();
                ViewBag.ErrorMessage = ex.Message;

                return View("Register", model);
            }
        }

        private bool UseCaptchaOnRegistration()
        {
            if (DebugHelper.IsDebug)
            {
                return false;
            }

            var tenancyName = _tenancyNameFinder.GetCurrentTenancyNameOrNull();
            if (tenancyName.IsNullOrEmpty())
            {
                return true;
            }

            var tenant = AsyncHelper.RunSync(() => GetActiveTenantAsync(tenancyName));
            return SettingManager.GetSettingValueForTenant<bool>(AppSettings.UserManagement.UseCaptchaOnRegistration, tenant.Id);
        }

        private void CheckSelfRegistrationIsEnabled()
        {
            if (!IsSelfRegistrationEnabled())
            {
                throw new UserFriendlyException(L("SelfUserRegistrationIsDisabledMessage_Detail"));
            }
        }

        private bool IsSelfRegistrationEnabled()
        {
            var tenancyName = _tenancyNameFinder.GetCurrentTenancyNameOrNull();
            if (tenancyName.IsNullOrEmpty())
            {
                return true;
            }

            var tenant = AsyncHelper.RunSync(() => GetActiveTenantAsync(tenancyName));
            return SettingManager.GetSettingValueForTenant<bool>(AppSettings.UserManagement.AllowSelfRegistration, tenant.Id);
        }

        #endregion

        #region ForgotPassword / ResetPassword

        public ActionResult ForgotPassword()
        {
            ViewBag.IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled;
            ViewBag.TenancyName = _tenancyNameFinder.GetCurrentTenancyNameOrNull();

            return View();
        }

        [HttpPost]
        [UnitOfWork]
        [ValidateAntiForgeryToken]
        public virtual async Task<JsonResult> SendPasswordResetLink(SendPasswordResetLinkViewModel model)
        {
            CheckModelState();

            _unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant);

            var user = await GetUserByChecking(model.EmailAddress, model.TenancyName);
            user.SetNewPasswordResetCode();
            await _userEmailer.SendPasswordResetLinkAsync(user);

            return Json(new MvcAjaxResponse());
        }

        [UnitOfWork]
        public virtual async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            CheckModelState();

            _unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant);

            var userId = Convert.ToInt64(SimpleStringCipher.Decrypt(model.UserId));

            var user = await _userManager.GetUserByIdAsync(userId);
            if (user == null || user.PasswordResetCode.IsNullOrEmpty() || user.PasswordResetCode != model.ResetCode)
            {
                throw new UserFriendlyException(L("InvalidPasswordResetCode"), L("InvalidPasswordResetCode_Detail"));
            }

            return View(model);
        }

        [HttpPost]
        [UnitOfWork]
        public virtual async Task<ActionResult> ResetPassword(ResetPasswordFormViewModel model)
        {
            CheckModelState();

            _unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant);

            var userId = Convert.ToInt64(SimpleStringCipher.Decrypt(model.UserId));

            var user = await _userManager.GetUserByIdAsync(userId);
            if (user == null || user.PasswordResetCode.IsNullOrEmpty() || user.PasswordResetCode != model.ResetCode)
            {
                throw new UserFriendlyException(L("InvalidPasswordResetCode"), L("InvalidPasswordResetCode_Detail"));
            }

            user.Password = new PasswordHasher().HashPassword(model.Password);
            user.PasswordResetCode = null;
            user.IsEmailConfirmed = true;
            user.ShouldChangePasswordOnNextLogin = false;

            await _userManager.UpdateAsync(user);

            if (user.IsActive)
            {
                await SignInAsync(user);
            }

            return RedirectToAction("Index", "Application");
        }

        #endregion

        #region Email activation / confirmation

        public ActionResult EmailActivation()
        {
            ViewBag.IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled;
            ViewBag.TenancyName = _tenancyNameFinder.GetCurrentTenancyNameOrNull();

            return View();
        }

        [HttpPost]
        [UnitOfWork]
        [ValidateAntiForgeryToken]
        public virtual async Task<JsonResult> SendEmailActivationLink(SendEmailActivationLinkViewModel model)
        {
            _unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant);

            var user = await GetUserByChecking(model.EmailAddress, model.TenancyName);

            user.SetNewEmailConfirmationCode();
            await _userEmailer.SendEmailActivationLinkAsync(user);

            return Json(new MvcAjaxResponse());
        }

        [UnitOfWork]
        public virtual async Task<ActionResult> EmailConfirmation(EmailConfirmationViewModel model)
        {
            CheckModelState();

            _unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant);

            var userId = Convert.ToInt64(SimpleStringCipher.Decrypt(model.UserId));

            var user = await _userManager.GetUserByIdAsync(userId);
            if (user == null || user.EmailConfirmationCode.IsNullOrEmpty() || user.EmailConfirmationCode != model.ConfirmationCode)
            {
                throw new UserFriendlyException(L("InvalidEmailConfirmationCode"), L("InvalidEmailConfirmationCode_Detail"));
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationCode = null;

            await _userManager.UpdateAsync(user);

            var tenancyName = user.TenantId.HasValue
                ? (await _tenantManager.GetByIdAsync(user.TenantId.Value)).TenancyName
                : "";

            return RedirectToAction(
                "Login",
                new
                                             {
                                                 successMessage = L("YourEmailIsConfirmedMessage"),
                                                 tenancyName = tenancyName,
                                                 userNameOrEmailAddress = user.UserName
                                             });
        }

        #endregion

        #region Child actions

        [ChildActionOnly]
        public PartialViewResult Languages()
        {
            return PartialView("~/Views/Account/_Languages.cshtml",
                new LanguagesViewModel
                {
                    AllLanguages = LocalizationManager.GetAllLanguages(),
                    CurrentLanguage = LocalizationManager.CurrentLanguage
                });
        }

        #endregion

        #region Private methods

        private async Task<User> GetUserByChecking(string emailAddress, string tenancyName)
        {
            var tenantId = await GetTenantIdOrDefault(tenancyName);
            var user = await _userManager.Users.Where(
                u => u.EmailAddress == emailAddress && u.TenantId == tenantId
                ).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new UserFriendlyException(L("InvalidEmailAddress"));
            }

            return user;
        }

        private async Task<int?> GetTenantIdOrDefault(string tenancyName)
        {
            return tenancyName.IsNullOrEmpty()
                ? AbpSession.TenantId
                : (await GetActiveTenantAsync(tenancyName)).Id;
        }

        private async Task<Tenant> GetActiveTenantAsync(string tenancyName)
        {
            var tenant = await _tenantManager.FindByTenancyNameAsync(tenancyName);
            if (tenant == null)
            {
                throw new UserFriendlyException(L("ThereIsNoTenantDefinedWithName{0}", tenancyName));
            }

            if (!tenant.IsActive)
            {
                throw new UserFriendlyException(L("TenantIsNotActive", tenancyName));
            }

            return tenant;
        }
        
        #endregion

        #region External Login

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ChallengeResult(
                provider,
                Url.Action(
                    "ExternalLoginCallback",
                    "Account",
                    new
                    {
                        ReturnUrl = returnUrl,
                        tenancyName = _tenancyNameFinder.GetCurrentTenancyNameOrNull() ?? ""
                    })
                );
        }

        [UnitOfWork]
        public virtual async Task<ActionResult> ExternalLoginCallback(string returnUrl, string tenancyName = "")
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            //Try to find tenancy name
            if (tenancyName.IsNullOrEmpty())
            {
                tenancyName = _tenancyNameFinder.GetCurrentTenancyNameOrNull();
                if (tenancyName.IsNullOrEmpty())
                {
                    var tenants = await FindPossibleTenantsOfUserAsync(loginInfo.Login);
                    switch (tenants.Count)
                    {
                        case 0:
                            return await RegisterView(loginInfo);
                        case 1:
                            tenancyName = tenants[0].TenancyName;
                            break;
                        default:
                            return View("TenantSelection", new TenantSelectionViewModel
                            {
                                Action = Url.Action("ExternalLoginCallback", "Account", new { returnUrl }),
                                Tenants = tenants.MapTo<List<TenantSelectionViewModel.TenantInfo>>()
                            });
                    }
                }
            }

            var loginResult = await _userManager.LoginAsync(loginInfo.Login, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    await SignInAsync(loginResult.User, loginResult.Identity, true);

                    if (string.IsNullOrWhiteSpace(returnUrl))
                    {
                        returnUrl = Url.Action("Index", "Application");
                    }

                    return Redirect(returnUrl);
                case AbpLoginResultType.UnknownExternalLogin:
                    return await RegisterView(loginInfo, tenancyName);
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, loginInfo.Email ?? loginInfo.DefaultUserName, tenancyName);
            }
        }

        private async Task<ActionResult> RegisterView(ExternalLoginInfo loginInfo, string tenancyName = null)
        {
            var name = loginInfo.DefaultUserName;
            var surname = loginInfo.DefaultUserName;

            var extractedNameAndSurname = TryExtractNameAndSurnameFromClaims(loginInfo.ExternalIdentity.Claims.ToList(), ref name, ref surname);

            var viewModel = new RegisterViewModel
            {
                TenancyName = tenancyName,
                EmailAddress = loginInfo.Email,
                Name = name,
                Surname = surname,
                IsExternalLogin = true
            };

            if (!tenancyName.IsNullOrEmpty() && extractedNameAndSurname)
            {
                return await Register(viewModel);
            }

            return RegisterView(viewModel);
        }

        [UnitOfWork]
        protected virtual async Task<List<Tenant>> FindPossibleTenantsOfUserAsync(UserLoginInfo login)
        {
            List<User> allUsers;
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                allUsers = await _userManager.FindAllAsync(login);
            }

            return allUsers
                .Where(u => u.TenantId != null)
                .Select(u => AsyncHelper.RunSync(() => _tenantManager.FindByIdAsync(u.TenantId.Value)))
                .ToList();
        }

        private static bool TryExtractNameAndSurnameFromClaims(List<Claim> claims, ref string name, ref string surname)
        {
            string foundName = null;
            string foundSurname = null;

            var givennameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
            if (givennameClaim != null && !givennameClaim.Value.IsNullOrEmpty())
            {
                foundName = givennameClaim.Value;
            }

            var surnameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
            if (surnameClaim != null && !surnameClaim.Value.IsNullOrEmpty())
            {
                foundSurname = surnameClaim.Value;
            }

            if (foundName == null || foundSurname == null)
            {
                var nameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                if (nameClaim != null)
                {
                    var nameSurName = nameClaim.Value;
                    if (!nameSurName.IsNullOrEmpty())
                    {
                        var lastSpaceIndex = nameSurName.LastIndexOf(' ');
                        if (lastSpaceIndex < 1 || lastSpaceIndex > (nameSurName.Length - 2))
                        {
                            foundName = foundSurname = nameSurName;
                        }
                        else
                        {
                            foundName = nameSurName.Substring(0, lastSpaceIndex);
                            foundSurname = nameSurName.Substring(lastSpaceIndex);
                        }
                    }
                }
            }

            if (!foundName.IsNullOrEmpty())
            {
                name = foundName;
            }

            if (!foundSurname.IsNullOrEmpty())
            {
                surname = foundSurname;
            }

            return foundName != null && foundSurname != null;
        }

        #endregion

        #region Impersonation

        [AbpMvcAuthorize(AppPermissions.Pages_Administration_Users_Impersonation)]
        public virtual async Task<JsonResult> Impersonate(ImpersonateModel model)
        {
            CheckModelState();

            if (AbpSession.ImpersonatorUserId.HasValue)
            {
                throw new UserFriendlyException(L("CascadeImpersonationErrorMessage"));
            }

            if (AbpSession.TenantId.HasValue)
            {
                if (!model.TenantId.HasValue)
                {
                    throw new UserFriendlyException(L("FromTenantToHostImpersonationErrorMessage"));
                }

                if (model.TenantId.Value != AbpSession.TenantId.Value)
                {
                    throw new UserFriendlyException(L("DifferentTenantImpersonationErrorMessage"));
                }
            }

            var result = await SaveImpersonationTokenAndGetTargetUrl(model.TenantId, model.UserId, false);
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return result;
        }

        [UnitOfWork]
        public virtual async Task<ActionResult> ImpersonateSignIn(string tokenId)
        {
            var cacheItem = await _cacheManager.GetImpersonationCache().GetOrDefaultAsync(tokenId);
            if (cacheItem == null)
            {
                throw new UserFriendlyException(L("ImpersonationTokenErrorMessage"));
            }

            //Switch to requested tenant
            using (_unitOfWorkManager.Current.SetFilterParameter(AbpDataFilters.MayHaveTenant, AbpDataFilters.Parameters.TenantId, cacheItem.TargetTenantId))
            {
                //Get the user from tenant
                var user = await _userManager.FindByIdAsync(cacheItem.TargetUserId);

                //Create identity
                var identity = await _userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);

                if (!cacheItem.IsBackToImpersonator)
                {
                    //Add claims for audit logging
                    if (cacheItem.ImpersonatorTenantId.HasValue)
                    {
                        identity.AddClaim(new Claim(AbpClaimTypes.ImpersonatorTenantId, cacheItem.ImpersonatorTenantId.Value.ToString(CultureInfo.InvariantCulture)));
                    }

                    identity.AddClaim(new Claim(AbpClaimTypes.ImpersonatorUserId, cacheItem.ImpersonatorUserId.ToString(CultureInfo.InvariantCulture)));
                }

                //Sign in with the target user
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);

                //Remove the cache item to prevent re-use
                await _cacheManager.GetImpersonationCache().RemoveAsync(tokenId);

                return RedirectToAction("Index", "Application");
            }
        }

        public virtual JsonResult IsImpersonatedLogin()
        {
            return Json(new MvcAjaxResponse { Result = AbpSession.ImpersonatorUserId.HasValue });
        }

        public virtual async Task<JsonResult> BackToImpersonator()
        {
            if (!AbpSession.ImpersonatorUserId.HasValue)
            {
                throw new UserFriendlyException(L("NotImpersonatedLoginErrorMessage"));
            }

            var result = await SaveImpersonationTokenAndGetTargetUrl(AbpSession.ImpersonatorTenantId, AbpSession.ImpersonatorUserId.Value, true);
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return result;
        }

        private async Task<JsonResult> SaveImpersonationTokenAndGetTargetUrl(int? tenantId, long userId, bool isBackToImpersonator)
        {
            //Create a cache item
            var cacheItem = new ImpersonationCacheItem(
                tenantId,
                userId,
                isBackToImpersonator
                );

            if (!isBackToImpersonator)
            {
                cacheItem.ImpersonatorTenantId = AbpSession.TenantId;
                cacheItem.ImpersonatorUserId = AbpSession.GetUserId();
            }

            //Create a random token and save to the cache
            var tokenId = Guid.NewGuid().ToString();
            await _cacheManager
                .GetImpersonationCache()
                .SetAsync(tokenId, cacheItem, TimeSpan.FromMinutes(1));

            //Find tenancy name
            string tenancyName = null;
            if (tenantId.HasValue)
            {
                tenancyName = (await _tenantManager.GetByIdAsync(tenantId.Value)).TenancyName;
            }

            //Create target URL
            var targetUrl = _webUrlService.GetSiteRootAddress(tenancyName) + "Account/ImpersonateSignIn?tokenId=" + tokenId;
            return Json(new MvcAjaxResponse { TargetUrl = targetUrl });
        }

        #endregion

        #region Linked Account

        [UnitOfWork]
        [AbpMvcAuthorize]
        public virtual async Task<JsonResult> SwitchToLinkedAccount(SwitchToLinkedAccountModel model)
        {
            CheckModelState();

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                if (!await _userLinkManager.AreUsersLinked(AbpSession.GetUserId(), model.TargetUserId))
                {
                    throw new ApplicationException(L("This account is not linked to your account"));
                }

                var targetUser = await _userManager.FindByIdAsync(model.TargetUserId);
                var targetTenantId = targetUser.TenantId;

                var result = await SaveAccountSwitchTokenAndGetTargetUrl(targetTenantId, model.TargetUserId);
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return result;
            }
        }

        [UnitOfWork]
        public virtual async Task<ActionResult> SwitchToLinkedAccountSignIn(string tokenId)
        {
            var cacheItem = await _cacheManager.GetSwitchToLinkedAccountCache().GetOrDefaultAsync(tokenId);
            if (cacheItem == null)
            {
                throw new UserFriendlyException(L("SwitchToLinkedAccountTokenErrorMessage"));
            }

            //Switch to requested tenant
            using (_unitOfWorkManager.Current.SetFilterParameter(AbpDataFilters.MayHaveTenant, AbpDataFilters.Parameters.TenantId, cacheItem.TargetTenantId))
            {
                //Get the user from tenant
                var user = await _userManager.FindByIdAsync(cacheItem.TargetUserId);

                //Create identity
                var identity = await _userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);

                //Add claims for audit logging
                if (cacheItem.ImpersonatorTenantId.HasValue)
                {
                    identity.AddClaim(new Claim(AbpClaimTypes.ImpersonatorTenantId, cacheItem.ImpersonatorTenantId.Value.ToString(CultureInfo.InvariantCulture)));
                }

                if (cacheItem.ImpersonatorUserId.HasValue)
                {
                    identity.AddClaim(new Claim(AbpClaimTypes.ImpersonatorUserId, cacheItem.ImpersonatorUserId.Value.ToString(CultureInfo.InvariantCulture)));
                }

                //Sign in with the target user
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);

                user.LastLoginTime = Clock.Now;

                //Remove the cache item to prevent re-use
                await _cacheManager.GetSwitchToLinkedAccountCache().RemoveAsync(tokenId);

                return RedirectToAction("Index", "Application");
            }
        }

        private async Task<JsonResult> SaveAccountSwitchTokenAndGetTargetUrl(int? targetTenantId, long targetUserId)
        {
            //Create a cache item
            var cacheItem = new SwitchToLinkedAccountCacheItem(
                targetTenantId,
                targetUserId,
                AbpSession.ImpersonatorTenantId,
                AbpSession.ImpersonatorUserId
                );

            //Create a random token and save to the cache
            var tokenId = Guid.NewGuid().ToString();
            await _cacheManager
                .GetSwitchToLinkedAccountCache()
                .SetAsync(tokenId, cacheItem, TimeSpan.FromMinutes(1));

            //Find tenancy name
            string tenancyName = null;
            if (targetTenantId.HasValue)
            {
                tenancyName = (await _tenantManager.GetByIdAsync(targetTenantId.Value)).TenancyName;
            }

            //Create target URL
            var targetUrl = _webUrlService.GetSiteRootAddress(tenancyName) + "Account/SwitchToLinkedAccountSignIn?tokenId=" + tokenId;
            return Json(new MvcAjaxResponse { TargetUrl = targetUrl });
        }

        #endregion

        #region Etc

        [AbpMvcAuthorize]
        public async Task<ActionResult> TestNotification(string message = "", string severity = "info")
        {
            if (message.IsNullOrEmpty())
            {
                message = "This is a test notification, created at " + Clock.Now;                
            }

            await _appNotifier.SendMessageAsync(
                AbpSession.GetUserId(),
                message,
                severity.ToPascalCase(CultureInfo.InvariantCulture).ToEnum<NotificationSeverity>()
                );

            return Content("Sent notification: " + message);
        }

        #endregion
    }
}