using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamMan.Models;

namespace TeamMan.Interfaces
{
    public interface INotificationService
    {
        public List<NotificationHub> getAllNotifications(string userId);
        public List<NotificationHub> GetNotificationHubsByUser(string userId);
        public  System.Threading.Tasks.Task openNotifications(string userId);
        public System.Threading.Tasks.Task readNotification(string userId , string taskId);




    }
}
