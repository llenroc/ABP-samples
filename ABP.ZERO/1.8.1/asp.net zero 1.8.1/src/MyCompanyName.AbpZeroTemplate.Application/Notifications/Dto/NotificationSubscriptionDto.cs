﻿using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Notifications;

namespace MyCompanyName.AbpZeroTemplate.Notifications.Dto
{
    public class NotificationSubscriptionDto : IDto
    {
        [Required]
        [MaxLength(NotificationInfo.MaxNotificationNameLength)]
        public string Name { get; set; }

        public bool IsSubscribed { get; set; }
    }
}