using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamMan.Interfaces;
using TeamMan.Models;
using Microsoft.EntityFrameworkCore;

namespace TeamMan.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IRepository<Models.NotificationHub> _NotRepository;
        private readonly ApplicationContext _context;

        public NotificationService(IRepository<Models.NotificationHub> repository ,
            ApplicationContext context )
        {
            _NotRepository = repository;
            _context = context;
        }

        public List<NotificationHub> getAllNotifications(string id)
        {
            return _context.Notifications.Where(p => p.UserId.Equals(id)).Include(p => p.User).OrderByDescending(p => p.createOn).ToList() ;
        }

        public List<NotificationHub> GetNotificationHubsByUser(string userId)
        {
            var oneWeekBeforeDate = DateTime.Now.Date.AddDays(-7);
            var nots = getAllNotifications(userId).Where(p => p.createOn > oneWeekBeforeDate).OrderByDescending(p => p.createOn).ToList();
            return nots;
        }

        public async System.Threading.Tasks.Task openNotifications(string userId)
        {
            var nots = this.GetNotificationHubsByUser(userId);
            foreach(var n in nots)
            {
                n.opened = true;
            }
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task readNotification(string userId, string taskId)
        {
            var nots = this.GetNotificationHubsByUser(userId).Where(p => p.objectId == Guid.Parse(taskId)).FirstOrDefault();
            if(nots != null)
            {
                nots.read = true;
                await _context.SaveChangesAsync();
            }
           

        }

    }
}
