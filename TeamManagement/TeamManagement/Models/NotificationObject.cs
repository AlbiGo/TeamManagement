using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class NotificationObject : Entity
    {
        [ForeignKey("Notification")]
        public Guid NotificationId { get; set; }
        public Guid ObjectId { get; set; }
        public virtual NotificationHub Notification {get;set;}
    }
}
