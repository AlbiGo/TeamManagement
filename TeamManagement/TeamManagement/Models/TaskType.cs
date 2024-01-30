using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class TaskType
    {
        [Key]
        public int Id { get; set; }
        public string type { get; set; } //Task ,Bug,Feauture ,UserStory

    }
}
