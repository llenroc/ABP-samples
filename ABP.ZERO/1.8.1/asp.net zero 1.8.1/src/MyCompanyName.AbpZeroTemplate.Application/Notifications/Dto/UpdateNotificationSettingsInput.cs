using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace MyCompanyName.AbpZeroTemplate.Notifications.Dto
{
    public class UpdateNotificationSettingsInput : IInputDto
    {
        public bool ReceiveNotifications { get; set; }

        public List<NotificationSubscriptionDto> Notifications { get; set; }
    }
}