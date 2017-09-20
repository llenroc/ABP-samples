using System.Threading.Tasks;
using Abp.Notifications;
using MyCompanyName.AbpZeroTemplate.Authorization.Users;

namespace MyCompanyName.AbpZeroTemplate.Notifications
{
    public interface IAppNotifier
    {
        Task WelcomeToTheApplicationAsync(User user);

        Task NewUserRegisteredAsync(User user);

        Task SendMessageAsync(long userId, string message, NotificationSeverity severity = NotificationSeverity.Info);
    }
}
