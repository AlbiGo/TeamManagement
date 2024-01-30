using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TeamMan.Interfaces
{
    public interface INotificationProducercs
    {
        public  void SendNotifications(Models.Task task, string message, string title);
        public void SendNotificationsToSpecificUser(string userId,Models.Task task, string message, string title);
        public Task SendSimpleNotificationsToSpecificUser(string userID, string message, string title);


    }

}
