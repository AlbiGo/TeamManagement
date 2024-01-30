using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class NotificationMessage 
    {
        [Key]
        public Guid id { get; set; }   
        public string message { get; set; }
        [ForeignKey("Sender")]
        public string SenderId { get; set; }
        [ForeignKey("Receiver")]
        public string ReceiverId { get; set; }
        [ForeignKey("Notification")]
        public Guid NotificationId { get; set; }
        public virtual ApplicationUser Sender { get; set; }
        public virtual ApplicationUser Receiver { get; set; }
        public virtual NotificationHub Notification { get; set; }



    }
}
