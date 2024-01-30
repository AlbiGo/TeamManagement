using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class Error : Entity
    {
        public string ErrorMessage { get; set; }
        public Error()
        {
            createdOn = DateTime.Now;
        }

        public void registerExeption(Exception ex)
        {
            ErrorMessage = ex.Message;
            Description = ex.Data.ToString();
        }
    }
}
