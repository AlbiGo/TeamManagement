using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class NotificationHub : Hub
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createOn { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public string articleHeading { get; set; }
        public string articleContent { get; set; }
        public virtual ApplicationUser User { get; set; }
        public bool opened { get; set; }
        public bool read { get; set; }

        public Nullable<Guid> objectId { get; set; }
        public int notificationType { get; set; }


        public NotificationHub()
        {
            createOn = DateTime.Now;
            opened = false;
            read = false;
        }
        //
        // Summary:
        //     Gets or sets an object that can be used to invoke methods on the clients connected
        //     to this hub.
        [NotMapped]
        public IHubCallerClients Clients { get; set; }
        //
        // Summary:
        //     Gets or sets the hub caller context.
        [NotMapped]
        public HubCallerContext Context { get; set; }
        //
        // Summary:
        //     Gets or sets the group manager.
        [NotMapped]
        public IGroupManager Groups { get; set; }

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}
