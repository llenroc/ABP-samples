using System;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Net.Mail;
using MyCompanyName.AbpZeroTemplate.Emailing;
using MyCompanyName.AbpZeroTemplate.MultiTenancy;
using MyCompanyName.AbpZeroTemplate.Security;
using MyCompanyName.AbpZeroTemplate.Web;

namespace MyCompanyName.AbpZeroTemplate.Authorization.Users
{
    /// <summary>
    /// Used to send email to users.
    /// </summary>
    public class UserEmailer : AbpZeroTemplateServiceBase, IUserEmailer, ITransientDependency
    {
        private readonly IEmailTemplateProvider _emailTemplateProvider;
        private readonly IEmailSender _emailSender;
        private readonly IWebUrlService _webUrlService;
        private readonly IRepository<Tenant> _tenantRepository;

        public UserEmailer(IEmailTemplateProvider emailTemplateProvider, IEmailSender emailSender,IWebUrlService webUrlService, IRepository<Tenant> tenantRepository)
        {
            _emailTemplateProvider = emailTemplateProvider;
            _emailSender = emailSender;
            _webUrlService = webUrlService;
            _tenantRepository = tenantRepository;
        }

        /// <summary>
        /// Send email activation link to user's email address.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="plainPassword">
        /// Can be set to user's plain password to include it in the email.
        /// </param>
        public async Task SendEmailActivationLinkAsync(User user, string plainPassword = null)
        {
            if (user.EmailConfirmationCode.IsNullOrEmpty())
            {
                throw new ApplicationException("EmailConfirmationCode should be set in order to send email activation link.");
            }

            var tenancyName = user.TenantId.HasValue
                ? _tenantRepository.Get(user.TenantId.Value).TenancyName
                : null;

            var link = _webUrlService.GetSiteRootAddress(tenancyName) + "Account/EmailConfirmation" +
                       "?userId=" + Uri.EscapeDataString(SimpleStringCipher.Encrypt(user.Id.ToString())) +
                       "&confirmationCode=" + Uri.EscapeDataString(user.EmailConfirmationCode);
            
            var emailTemplate = new StringBuilder(_emailTemplateProvider.GetDefaultTemplate());
            emailTemplate.Replace("{EMAIL_TITLE}", L("EmailActivation_Title"));
            emailTemplate.Replace("{EMAIL_SUB_TITLE}", L("EmailActivation_SubTitle"));

            var mailMessage = new StringBuilder();

            mailMessage.AppendLine("<b>" + L("NameSurname") + "</b>: " + user.Name + " " + user.Surname + "<br />");
            
            if (!tenancyName.IsNullOrEmpty())
            {
                mailMessage.AppendLine("<b>" + L("TenancyName") + "</b>: " + tenancyName + "<br />");
            }

            mailMessage.AppendLine("<b>" + L("UserName") + "</b>: " + user.UserName + "<br />");
            
            if (!plainPassword.IsNullOrEmpty())
            {
                mailMessage.AppendLine("<b>" + L("Password") + "</b>: " + plainPassword + "<br />");
            }

            mailMessage.AppendLine("<br />");
            mailMessage.AppendLine(L("EmailActivation_ClickTheLinkBelowToVerifyYourEmail") + "<br /><br />");
            mailMessage.AppendLine("<a href=\"" + link + "\">" + link + "</a>");

            emailTemplate.Replace("{EMAIL_BODY}", mailMessage.ToString());

            await _emailSender.SendAsync(user.EmailAddress, L("EmailActivation_Subject"), emailTemplate.ToString());
        }

        /// <summary>
        /// Sends a password reset link to user's email.
        /// </summary>
        /// <param name="user">User</param>
        public async Task SendPasswordResetLinkAsync(User user)
        {
            if (user.PasswordResetCode.IsNullOrEmpty())
            {
                throw new ApplicationException("PasswordResetCode should be set in order to send password reset link.");
            }

            var tenancyName = user.TenantId.HasValue
                ? _tenantRepository.Get(user.TenantId.Value).TenancyName
                : null;

            var link = _webUrlService.GetSiteRootAddress(tenancyName) + "Account/ResetPassword" +
                       "?userId=" + Uri.EscapeDataString(SimpleStringCipher.Encrypt(user.Id.ToString())) +
                       "&resetCode=" + Uri.EscapeDataString(user.PasswordResetCode);

            var emailTemplate = new StringBuilder(_emailTemplateProvider.GetDefaultTemplate());
            emailTemplate.Replace("{EMAIL_TITLE}", L("PasswordResetEmail_Title"));
            emailTemplate.Replace("{EMAIL_SUB_TITLE}", L("PasswordResetEmail_SubTitle"));

            var mailMessage = new StringBuilder();

            mailMessage.AppendLine("<b>" + L("NameSurname") + "</b>: " + user.Name + " " + user.Surname + "<br />");

            if (!tenancyName.IsNullOrEmpty())
            {
                mailMessage.AppendLine("<b>" + L("TenancyName") + "</b>: " + tenancyName + "<br />");
            }

            mailMessage.AppendLine("<b>" + L("UserName") + "</b>: " + user.UserName + "<br />");

            mailMessage.AppendLine("<br />");
            mailMessage.AppendLine(L("PasswordResetEmail_ClickTheLinkBelowToResetYourPassword") + "<br /><br />");
            mailMessage.AppendLine("<a href=\"" + link + "\">" + link + "</a>");

            emailTemplate.Replace("{EMAIL_BODY}", mailMessage.ToString());

            await _emailSender.SendAsync(user.EmailAddress, L("PasswordResetEmail_Subject"), emailTemplate.ToString());
        }
    }
}