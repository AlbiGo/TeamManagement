using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class Project
    {
        [Key]
        public Guid Id { get; set; }
        public string Projectname { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string References { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime Deadline { get; set; }
        [ForeignKey("ProjectManager")]
        public string ProjectManagerID { get; set; }
        public string Priority { get; set; }

        public DateTime createdOn { get; set; }
        [ForeignKey("Client")]
        public Nullable<Guid> ClientId { get; set; }
        public DateTime ModifiedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
        public string designPath { get; set; }
        [ForeignKey("Organisation")]
        public Guid OrganisationID { get; set; }
        public Project()
        {
            createdOn = DateTime.Now.Date;
            Status = "created";
        }
        public virtual Organisation Organisation { get; set; }

        //public ICollection<Task> Tasks { get; set; }
        public virtual Client Client { get; set; }

        public virtual ApplicationUser ProjectManager { get; set; }
    }
}
