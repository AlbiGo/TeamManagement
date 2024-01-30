using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class Right
    {
            [Key]
            public Guid RightId { get; set; }
            [ForeignKey("Role")]
            public int RoleID { get; set; }
            public bool delete { get; set; }
            public bool update { get; set; }
            public bool create { get; set; }
            public bool read { get; set; }

            public bool userActions { get; set; }
            //[ForeignKey("Id")]
            public virtual ICollection<IdentityRole> Roles { get; set; }



        
    }
}
