using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class UserStatus
    {
        [Key]
        public int Id { get; set; }
        public string status { get; set; }

    }
}
