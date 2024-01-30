using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class Bug : Entity
    {
        [ForeignKey("Task")]
        public Guid taskId { get; set; }
        public DateTime Deadline { get; set; }
        public Bug()
        {
            createdOn = DateTime.Now;
        }
    }
}
