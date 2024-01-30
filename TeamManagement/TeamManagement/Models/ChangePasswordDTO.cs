using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class ChangePasswordDTO
    {
        public string userId { get; set; }
        public string password { get; set; }
    }
}
