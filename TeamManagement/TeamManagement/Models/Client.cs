using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class Client : Entity
    {
        public string ClientName { get; set; }
        public string email { get; set; }
        public string PhoneNumber { get; set; }
        public string Addres { get; set; }
    }
}
