using Microsoft.AspNetCore.Identity;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class ApplicationUser : IdentityUser

    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Address { get; set; }
        public DateTime createdOn { get; set; }
        public int DepartmentID { get; set; }
        public int TeamId { get; set; }

        public string profileUrl { get; set; }

        public string Description { get; set; }
        [ForeignKey("TeamRoles")]
        public Guid TeamRoleID { get; set; }
        [ForeignKey("Organisation")]
        public Guid OrganisationID { get; set; }
        [ForeignKey("Roles")]
        public string RoleID { get; set; }
        public bool isActive { get; set; }
        public bool isDemo { get; set; }
        public DateTime ExpireData { get; set; }

        public virtual TeamRoles TeamRoles { get; set; }
        public virtual IdentityRole Roles { get; set; }
        public virtual Organisation Organisation { get; set; }


        public ApplicationUser()
        {
            isActive = false;
            createdOn = DateTime.Now;
            if(isDemo == true)
            {
                var ExpireDate = this.createdOn.AddDays(30);
                ExpireData = ExpireDate;
            }
         
        }


    }
}
