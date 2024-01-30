using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class Comment : Entity
    {
        public string comment {get;set;}
        [ForeignKey("Task")]
        public Guid TaskId { get; set; }

        public Task tasks { get; set; }

        public Comment()
        {
            createdOn = DateTime.Now;

        }
    }
}
