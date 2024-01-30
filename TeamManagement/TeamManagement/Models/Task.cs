using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class Task
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("Types")]
        public int TypeId { get; set; }
        public int taskNumber { get; set; }
        public string TaskTitle { get; set; }
        public string Description { get; set; }
        [ForeignKey("Project")]
        public Guid ProjectId { get; set; }
        [ForeignKey("TaskStatus")]
        public int Status { get; set; }
        public string References { get; set; }
        [ForeignKey("User")]
        public string UserID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime closedOn { get; set; }
        [ForeignKey("Priority")]
        public int TaskPriority { get; set; }

        public DateTime createdOn { get; set; }

        public DateTime ModifiedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
        public bool isCompleted { get; set; }
        public bool isActive { get; set; }

        public string designPath { get; set; }
        public string TaskDocumentPath { get; set; }

        public string Comment { get; set; }

        public Task()
        {
            createdOn = DateTime.Now;
            Status = 1;
            isCompleted = false;
            isActive = true;
        }

        public ApplicationUser User { get; set; }
        public Project Projects { get; set; }
        public TaskStatus TaskStatus {get;set;}
        public virtual TaskType Types { get; set; }
        public Priority Priority { get; set; }

        public virtual List<Comment> Comments { get; set; }
        public virtual List<Subtask> Subtasks { get; set; }



    }
}
