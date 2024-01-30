using Lib.Net.Http.WebPush;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class SubsriptionUser : PushSubscription 
    {
        public Guid Id { get; set; }
        [ForeignKey("User")]
        public string userId { get; set; }
        public DateTime createdOn { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
