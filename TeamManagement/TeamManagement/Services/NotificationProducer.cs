using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TeamMan.Interfaces;
using static TeamMan.Startup;
using TeamMan.Models;
using Microsoft.AspNetCore.SignalR;
using static TeamMan.Models.AngularPushNotification;
using Microsoft.EntityFrameworkCore;

namespace TeamMan.Services
{
    public class NotificationProducer : INotificationProducercs 
    {
      //  private readonly IPushSubscriptionsService _pushSubscriptionsService;
        private readonly PushServiceClient _pushClient;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly IHubContext<NotificationUserHub> _notificationUserHubContext;
        private readonly IUserConnectionManager _userConnectionManager;
        private readonly ApplicationContext _applicationContext;
        private UserInterface<ApplicationUser> _userInterface;



        public NotificationProducer(
            IOptions<PushNotificationsOptions> options, 
            IHubContext<NotificationHub> hubContext ,
          //  IPushSubscriptionsService pushSubscriptionsService,
            IHubContext<NotificationUserHub> notificationUserHubContext,
            IUserConnectionManager userConnectionManager,
            ApplicationContext applicationContext,
            PushServiceClient pushClient,
            UserInterface<ApplicationUser> userInterface)
        {
          //  _pushSubscriptionsService = pushSubscriptionsService;
            _pushClient = pushClient;
            _pushClient.DefaultAuthentication = new VapidAuthentication(options.Value.PublicKey, options.Value.PrivateKey)
            {
                Subject = "https://angular-aspnetmvc-pushnotifications.demo.io"
            };
            _notificationHubContext = hubContext;
            _notificationUserHubContext = notificationUserHubContext;
            _userConnectionManager = userConnectionManager;
            _applicationContext = applicationContext;
            _userInterface = userInterface;

        }
      

        public void SendNotifications(Models.Task task,  string message,string title)
        {
            List<NotificationHub> notifications = new List<NotificationHub>()
            {
                new NotificationHub()
                {
                    articleHeading = title,
                    articleContent = message
                },
                new NotificationHub()
                {
                    articleHeading = title,
                    articleContent = message
                }
            };

            // send notification to all users
            _notificationHubContext.Clients.All.SendAsync("sendToUser", notifications);
        }
        public void SendNotificationsToSpecificUser(string userID,Models.Task task, string message, string title)
        {
            List<NotificationHub> notifications = _applicationContext.Notifications.Where(p => p.UserId.Equals(userID)).Include(p => p.User).ToList();


            var connections = _userConnectionManager.GetUserConnections(userID);
            if (connections != null && connections.Count > 0)
            {
                foreach (var connectionId in connections)
                {
                     _notificationUserHubContext.Clients.Client(connectionId).SendAsync("sendToSpecificUser", notifications);
                }
            }
            // send notification to specific user
            //_notificationHubContext.Clients.Client("").SendAsync("sendToUser", notification.Topic, notification.Content);
        }

        public async System.Threading.Tasks.Task SendSimpleNotificationsToSpecificUser(string userID, string message, string title)
        {
          
                List<NotificationHub> notifications = _applicationContext.Notifications.Where(p => p.UserId.Equals(userID)).ToList();
                var connections = _userConnectionManager.GetUserConnections(userID);
                if (connections != null && connections.Count > 0)
                {
                    foreach (var connectionId in connections)
                    {
                       await _notificationUserHubContext.Clients.Client(connectionId).SendAsync("userDeactivated", notifications);
                    }
                }
            


            
            // send notification to specific user
            //_notificationHubContext.Clients.Client("").SendAsync("sendToUser", notification.Topic, notification.Content);
        }

        public void SendNotificationsTeamLeader(ApplicationUser user, string message, string title)
        {

            var teamLeader = this._userInterface.getTeamLeader();
            List<NotificationHub> notifications = _applicationContext.Notifications.Where(p => p.UserId.Equals(teamLeader.Id)).ToList();
            var connections = _userConnectionManager.GetUserConnections(teamLeader.Id);
            if (connections != null && connections.Count > 0)
            {
                foreach (var connectionId in connections)
                {
                    _notificationUserHubContext.Clients.Client(connectionId).SendAsync("sendToSpecificUser", notifications);
                }
            }
            // send notification to specific user
            //_notificationHubContext.Clients.Client("").SendAsync("sendToUser", notification.Topic, notification.Content);
        }
        public void SendNotificationsToAdmin(ApplicationUser user, string message, string title)
        {

            var teamAdmin = this._userInterface.getTeamAdmin();
            List<NotificationHub> notifications = _applicationContext.Notifications.Where(p => p.UserId.Equals(teamAdmin.Id)).ToList();
            var connections = _userConnectionManager.GetUserConnections(teamAdmin.Id);
            if (connections != null && connections.Count > 0)
            {
                foreach (var connectionId in connections)
                {
                    _notificationUserHubContext.Clients.Client(connectionId).SendAsync("sendToSpecificUser", notifications);
                }
            }
            // send notification to specific user
            //_notificationHubContext.Clients.Client("").SendAsync("sendToUser", notification.Topic, notification.Content);
        }

       
    }
}
