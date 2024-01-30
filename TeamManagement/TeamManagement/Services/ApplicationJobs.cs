using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TeamMan.Interfaces;
using TeamMan.Models;

namespace TeamMan.Services
{
    public class ApplicationJobs : IHostedService, IDisposable
    {
        private  Timer _timer;
        //private SignInManager<ApplicationUser> _signManager;
        //private RoleManager<IdentityRole> _RoleManager;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
        public IServiceScopeFactory Services { get; }

        public ApplicationJobs(IServiceScopeFactory services)
        {
            Services = services;
        }
        public System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            // todo:
            // timer repeates call to RemoveScheduledAccounts every 24 hours.
            _timer = new Timer(
           SystemJob,
           null,
           TimeSpan.Zero,
           TimeSpan.FromHours(24)
             );
            //_timer = new Timer(
            //SendTaskNot,
            //null,
            //TimeSpan.Zero,
            //TimeSpan.FromHours(24)
            // );

            return System.Threading.Tasks.Task.CompletedTask;

        }
       

        public void  SystemJob(object state)
        {

            using (var scope = Services.CreateScope())
            {
                var _applicationContext = scope.ServiceProvider.GetService<ApplicationContext>();
                var _userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                var notProducer = scope.ServiceProvider.GetService<INotificationProducercs>();
                var users = _userManager.Users.Where(p => p.isDemo == true && p.isActive == true).ToList();
                var createdUserData = new DateTime();
                foreach (var u in users)
                {
                    createdUserData = u.createdOn;
                    var dateAfter20days = createdUserData.AddDays(23);
                    var message = "Your trial expires on one week";
                    if (DateTime.Now == dateAfter20days)
                    {
                        NotificationHub notification = new NotificationHub()
                        {
                            articleContent = message,
                            articleHeading = message,
                            UserId = u.Id,
                            notificationType = 1,
                            User = u
                        };

                        _applicationContext.Notifications.Add(notification);
                        _applicationContext.SaveChangesAsync();
                        notProducer.SendSimpleNotificationsToSpecificUser(u.Id, message, message);
                    }
                    if (u.ExpireData == DateTime.Now)
                    {
                        u.isActive = false;
                        _applicationContext.SaveChangesAsync();

                    }
                }
                var tasks = _applicationContext.Tasks.Where(p => p.isActive == true && p.isCompleted == false).ToList();
                foreach (var t in tasks)
                {
                    var dateNow = DateTime.Now;
                    if (t.Deadline > dateNow)
                    {
                        if ((t.Deadline - dateNow).TotalDays <= 3)
                        {
                            string message = "";
                            if((t.Deadline - dateNow).TotalDays < 1)
                            {
                                 message = "!!!Deadline for Task " + t.taskNumber + "  is today .";
                            }
                            else
                            {
                                int daysToDealine = (int)(t.Deadline - dateNow).TotalDays;
                                message = "!!!Deadline for Task " + t.taskNumber + "  is in " + daysToDealine + " days .";

                            }
                            NotificationHub notification = new NotificationHub()
                            {
                                articleContent = message,
                                articleHeading = message,
                                UserId = t.UserID,
                                notificationType = 1,
                                User = t.User,
                                objectId = t.Id
                            };

                            _applicationContext.Notifications.Add(notification);
                            _applicationContext.SaveChangesAsync();
                            notProducer.SendSimpleNotificationsToSpecificUser(t.UserID, message, message);

                        }
                    }

                }



            }

        }

        //public void SendTaskNot(object state)
        //{
        //    using (var scope = Services.CreateScope())
        //    {
        //        var tasks = new List<Models.Task>();
        //        var _applicationContext = scope.ServiceProvider.GetService<ApplicationContext>();
        //        using (_applicationContext)
        //        {
        //             tasks = _applicationContext.Tasks.Where(p => p.isActive == false && p.isCompleted == false).ToList();
        //        }
        //        var notProducer = scope.ServiceProvider.GetService<INotificationProducercs>();
               
                
        //    }

        //}

        public System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            // todo:
            _timer?.Change(Timeout.Infinite, 0);

            return System.Threading.Tasks.Task.CompletedTask;
        }
        public virtual void Dispose()
        {
            _stoppingCts.Cancel();
        }

    }
}
