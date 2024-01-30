using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamMan.Models;

namespace TeamMan.ViewModels
{
    public class GlobalSearchDTO
    {
        public List<ApplicationUser> Accounts { get; set; }
        public List<Project> Projects { get; set; }
        public List<Models.Task> Tasks { get; set; }
    }
}
