using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class Log : Exception
    {
        [Key]
        public Guid Id { get; set; }
        public string LogMessage { get; set; }
        public string Description { get; set; }
        public DateTime createdOn { get; set; }
        public Log()
        {
            createdOn = DateTime.Now;

        }
        //public void registerExeption(Exception ex)
        //{
        //    LogMessage = ex.Message;
        //    Description = ex.Data.ToString();
        //}
        
    }

    
}
