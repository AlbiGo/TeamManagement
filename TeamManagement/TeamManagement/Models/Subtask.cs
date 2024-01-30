using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class Subtask : Entity 
    {
        [ForeignKey("Task")]
        public Guid parentTaskId  { get; set; }
        public string taskTitle { get; set; }
        public virtual Task Task { get; set; }

        public bool isCompleted { get; set; }
        public bool isActive { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public string References { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime closedOn { get; set; }
        public string Priority { get; set; }
        public Subtask()
        {
            createdOn = DateTime.Now;
            Status = 1;
            isCompleted = false;
            isActive = true;
        }


    }
}
