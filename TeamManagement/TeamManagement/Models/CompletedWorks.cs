using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class CompletedWorks : Entity
    {
        [ForeignKey("Task")]
        public Guid taskId { get; set; }
        public string Title { get; set; }
        public string completedTaskpath { get; set; }
        [ForeignKey("Id")]
        public string completedBy { get; set; }
        public DateTime completedOn { get; set; }
        public virtual Task Task { get; set; }

        public virtual ApplicationUser CompletedBy { get; set; }
    }
}
