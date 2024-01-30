using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class TeamRoles
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        [ForeignKey("Role")]
        public string roleId { get; set; }

        public IdentityRole Role { get; set; }
      
    }
}
