using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
       

        public ApplicationContext(DbContextOptions options)

            : base(options)

        {

        }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Subtask> Subtasks { get; set; }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Right> Rights { get; set; }
        public DbSet<TeamRoles> TeamRoles { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Priority> Priority { get; set; }

        public DbSet<Error> Errors { get; set; }
        public DbSet<Bug> Bugs { get; set; }
        public DbSet<TaskType> Types { get; set; }

        public DbSet<NotificationHub> Notifications { get; set; }
        public DbSet<TaskStatus> TaskStatuses { get; set; }
        public DbSet<UserStatus> userStatuses { get; set; }
        public DbSet<CompletedWorks> CompletedWorks { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Organisation> Organisations { get; set; }

        public DbSet<NotificationObject> NotificationObjects { get; set; }


        


        //  public DbSet<SubsriptionUser> Subsriptions { get; set; }





    }
}
