using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class Entity
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createdOn { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }

        public string Description { get; set; }
        public bool isActive { get; set; }

        public Entity()
        {
            this.createdOn = DateTime.Now;
        }
    }
}
