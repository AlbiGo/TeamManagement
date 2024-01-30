using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class Organisation : Entity
    {
        public string OrganisationName { get; set; }
        public bool isDemo { get; set; }
        public string Address { get; set; }
        public int members { get; set; }
        [ForeignKey("User")]
        public string owner { get; set; }
        public ApplicationUser User { get; set; }
    }
}
