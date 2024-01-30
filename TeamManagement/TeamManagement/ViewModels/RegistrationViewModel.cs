using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.ViewModels
{
    public class RegistrationViewModel //Registration DTO
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string phoneNumber { get; set; }
        public string password { get; set; }
        public int department { get; set; }
        public string teamRoleId { get; set; }
        public int team { get; set; }
        public string createdBy { get; set; }


    }
}
