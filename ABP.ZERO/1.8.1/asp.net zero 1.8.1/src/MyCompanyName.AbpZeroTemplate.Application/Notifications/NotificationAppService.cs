using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Configuration;
using Abp.Notifications;
using Abp.Runtime.Session;
using MyCompanyName.AbpZeroTemplate.Notifications.Dto;

namespace MyCompanyName.AbpZeroTemplate.Notifications
{
    //TODO: Create more unit tests
    [AbpAuthorize]
    public class NotificationAppService : AbpZeroTemplateAppServiceBase, INotificationAppService
    {
        private readonly INotificationDefinitionManager _notificationDefinitionManager;
        private readonly IUserNotificationManager _userNotificationManager;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;

        public NotificationAppService(
            INotificationDefinitionManager notificationDefinitionManager,
            IUserNotificationManager userNotificationManager, 
            INotificationSubscriptionManager notificationSubscriptionManager)
        {
            _notificationDefinitionManager = notificationDefinitionManager;
            _userNotificationManager = userNotificationManager;
            _notificationSubscriptionManager = notificationSubscriptionManager;
        }

        public async Task<GetNotificationsOutput> GetUserNotifications(GetUserNotificationsInput input)
        {
            var totalCount = await _userNotificationManager.GetUserNotificationCountAsync(
                AbpSession.GetUserId(), input.State
                );

            var unreadCount = await _userNotificationManager.GetUserNotificationCountAsync(
                AbpSession.GetUserId(), UserNotificationState.Unread
                );

            var notifications = await _userNotificationManager.GetUserNotificationsAsync(
                AbpSession.GetUserId(), input.State, input.SkipCount, input.MaxResultCount
                );

            return new GetNotificationsOutput(totalCount, unreadCount, notifications);
        }

        public async Task SetAllNotificationsAsRead()
        {
            await _userNotificationManager.UpdateAllUserNotificationStatesAsync(AbpSession.GetUserId(), UserNotificationState.Read);
        }

        public async Task SetNotificationAsRead(IdInput<Guid> input)
        {
            var userNotification = await _userNotificationManager.GetUserNotificationAsync(input.Id);
            if (userNotification.UserId != AbpSession.GetUserId())
            {
                throw new ApplicationException(string.Format("Given user notification id ({0}) is not belong to the current user ({1})", input.Id, AbpSession.GetUserId()));
            }

            await _userNotificationManager.UpdateUserNotificationStateAsync(input.Id, UserNotificationState.Read);
        }

        public async Task<GetNotificationSettingsOutput> GetNotificationSettings()
        {
            var output = new GetNotificationSettingsOutput();
            
            output.ReceiveNotifications = await SettingManager.GetSettingValueAsync<bool>(NotificationSettingNames.ReceiveNotifications);
            
            output.Notifications = (await _notificationDefinitionManager
                .GetAllAvailableAsync(AbpSession.TenantId, AbpSession.GetUserId()))
                .Where(nd => nd.EntityType == null) //Get general notifications, not entity related notifications.
                .MapTo<List<NotificationSubscriptionWithDisplayNameDto>>();
            
            var subscribedNotifications = (await _notificationSubscriptionManager
                .GetSubscribedNotificationsAsync(AbpSession.GetUserId()))
                .Select(ns => ns.NotificationName)
                .ToList();

            output.Notifications.ForEach(n => n.IsSubscribed = subscribedNotifications.Contains(n.Name));

            return output;
        }

        public async Task UpdateNotificationSettings(UpdateNotificationSettingsInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.GetUserId(), NotificationSettingNames.ReceiveNotifications, input.ReceiveNotifications.ToString());

            foreach (var notification in input.Notifications)
            {
                if (notification.IsSubscribed)
                {
                    await _notificationSubscriptionManager.SubscribeAsync(AbpSession.TenantId, AbpSession.GetUserId(), notification.Name);
                }
                else
                {
                    await _notificationSubscriptionManager.UnsubscribeAsync(AbpSession.GetUserId(), notification.Name);
                }
            }
        }
    }
}