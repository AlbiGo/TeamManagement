using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Crmf;
using TeamMan.Helpers;
using TeamMan.Interfaces;
using TeamMan.Models;
using TeamMan.ViewModels;

namespace TeamMan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private enum NotificationTypes
        {
            Task = 1,
            Event = 2,
            Simple = 3,
            OfficeTask = 4,
            Bug = 5
        }
        private readonly ApplicationContext _applicationContext;
        private readonly IRepository<Models.Task> _Repository;
        private readonly IRepository<Models.NotificationHub> _NotRepository;
        public LogHelper _error = new LogHelper();
        private readonly INotificationProducercs _notificationProducercs;
        private readonly string loggedUserID;
        private readonly Guid organisationID;
        private readonly IHttpContextAccessor _httpContextAccessor;
        // private readonly IPushSubscriptionsService _pushSubscriptionsService;
        private IHostingEnvironment _env;
        private RoleManager<IdentityRole> _RoleManager;


        public TaskController(ApplicationContext applicationContext,    //Constructor 
            IHttpContextAccessor httpContextAccessor,
            IRepository<Models.Task> repository ,
            INotificationProducercs notificationProducercs,
            IRepository<Models.NotificationHub> notRepository,
            IHostingEnvironment env,
            RoleManager<IdentityRole> RoleManager)
        {
            _applicationContext = applicationContext;
            _Repository = repository;
            _notificationProducercs = notificationProducercs;
            
       //     _pushSubscriptionsService = pushSubscriptionsService;
            _NotRepository = notRepository;
            _env = env;
            _RoleManager = RoleManager;
            _httpContextAccessor = httpContextAccessor;
            if (_httpContextAccessor.HttpContext.User.Claims.Count() > 0)
            {
                this.loggedUserID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name).Value;
                var loggedUser = _applicationContext.Users.Where(p => p.Id.Equals(this.loggedUserID)).FirstOrDefault();
                organisationID = loggedUser.OrganisationID;
            }

        }
        [HttpPost("~/api/createTask")]
        [Authorize(Roles = "admin,superadmin")]
        public async Task<IActionResult> createTask(TeamMan.Models.Task task)
        {
            try
            {
                EmailHelper emailHelper = new EmailHelper(_applicationContext, _env);
                if (ModelState.IsValid)
                {
                    
                    var project = _applicationContext.Projects.Where(p => p.Id == Guid.Parse("22fd6b34-67d1-45b7-991c-2104565ab2f1")).FirstOrDefault();
                    task.Projects = project;
                    var user = await _applicationContext.Users.FindAsync(task.UserID);
                   
                    if (user.isActive == false)
                    {
                        throw new Exception("User is not activated");
                    }
                    
                    task.User = user;
                    Random rnd = new Random();
                    int taskNumber = rnd.Next(1, 900);
                    task.taskNumber = taskNumber;
                    var createdBy = task.CreatedBy;
                    task.TaskPriority = 1;
                    task.TypeId = 1;
                    var userCreatedBy = _applicationContext.Users.Where(p => p.Id.Equals(createdBy.ToString())).FirstOrDefault();
                    var userToSend = _applicationContext.Users.Where(p => p.Id == task.UserID).FirstOrDefault();
                    _applicationContext.Add(task);
                    await _applicationContext.SaveChangesAsync();
                    string titleNot = "Task" + task.taskNumber + " has been assigned to you. " ;
                    string messageNot = "Task " + task.taskNumber + " is assigned to you";
                    //emailHelper.sendEmail(task, userToSend.Email, messageNot);
                    var Status = _applicationContext.TaskStatuses.Where(p => p.status == "new").First();
                    task.TaskStatus = Status;
                    string userID = this.loggedUserID;
                    NotificationHub notification = new NotificationHub()
                    {
                        articleContent = messageNot,
                        articleHeading = "Task " + task.taskNumber,
                        UserId = task.UserID,
                        objectId = task.Id,
                        notificationType = 1,
                        User = user
                    };

                    _NotRepository.Add(notification);
                    await _applicationContext.SaveChangesAsync();
                   // this._notificationProducercs.SendNotificationsToSpecificUser(task.UserID, task, messageNot, titleNot);
                 
                    return new OkResult();
                }
                else
                {
                    return new BadRequestResult();
                }

            }
            catch(Exception ex)
            {
                _error.registerExeption(ex , _applicationContext);
                return StatusCode(500, $"Internal server error: {ex}");
            }

        }
        [HttpPost("~/api/resolveTask")]
        [Authorize(Roles = "admin,superadmin")]
        public async Task<IActionResult> resolveTask(string tasktitle)
        {
            try
            {
                var task = this.getTasks().Where(p => p.Id == Guid.Parse(tasktitle)).FirstOrDefault();
                task.isCompleted = true;
                task.isActive = false;
                task.Status = 1002;
                task.closedOn = DateTime.Now;
                this._applicationContext.Update(task);
                await _applicationContext.SaveChangesAsync();
                var User = this._applicationContext.Users.Where(p => p.Id == task.UserID).FirstOrDefault();
                var appHelper = new AppHelper(_env);
                string path = appHelper.saveCompletedTasks(task);
                var completedWork = new CompletedWorks()
                {
                    CreatedBy = Guid.Parse(task.UserID),
                    Title = task.TaskTitle,
                    Description = task.Description,
                    completedTaskpath = path,
                    isActive = false,
                    Task = task

                };
                task.designPath = path;
                this._applicationContext.CompletedWorks.Add(completedWork);
                await _applicationContext.SaveChangesAsync();
                EmailHelper emailHelper = new EmailHelper(_applicationContext, _env);
                var userEmailAddres = _applicationContext.Users.Where(p => p.Id.Equals(task.UserID)).FirstOrDefault().Email;
                var message = "Task " + task.taskNumber + " has been accepted .";
                NotificationHub notification = new NotificationHub()
                {
                    articleContent = message,
                    articleHeading = "Task " + task.taskNumber,
                    UserId = task.UserID,
                    objectId = task.Id,
                    notificationType = 1
                };

                _NotRepository.Add(notification);
                await _applicationContext.SaveChangesAsync();
                emailHelper.sendEmail(task, userEmailAddres, message);
                this._notificationProducercs.SendNotificationsToSpecificUser(task.UserID, task, message, message);
                return new OkResult();
            }
            catch(Exception ex)
            {
                _error.registerExeption(ex,_applicationContext);
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [HttpPost("~/api/rejectTask")]
        [Authorize(Roles = "admin,superadmin")]
        public async Task<IActionResult> rejectTask(string tasktitle , string comment)
        {
            try
            { 
            var task = this.getTasks().Where(p => p.Id == Guid.Parse(tasktitle)).FirstOrDefault();
            task.isCompleted = false;
            task.isActive = true;
            task.Status = 5;
            task.Comment = comment;
            task.closedOn = DateTime.Now;
            this._applicationContext.Update(task);
            var User = this._applicationContext.Users.Where(p => p.Id == task.UserID).FirstOrDefault();
            EmailHelper emailHelper = new EmailHelper(_applicationContext, _env);
            var userEmailAddres = _applicationContext.Users.Where(p => p.Id.Equals(task.UserID)).FirstOrDefault().Email;
            var message = "Task " + task.taskNumber + " has been rejected .";
            NotificationHub notification = new NotificationHub()
            {
                articleContent = message,
                articleHeading = "Task " + task.taskNumber,
                UserId = task.UserID,
                objectId = task.Id,
                notificationType = 1
            };

            _NotRepository.Add(notification);
            await _applicationContext.SaveChangesAsync();
            emailHelper.sendEmail(task, userEmailAddres, message);
            this._notificationProducercs.SendNotificationsToSpecificUser(task.UserID, task, message, message);
            return new OkResult();
        }
            catch(Exception ex)
            {
                _error.registerExeption(ex, _applicationContext);
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [HttpPost("~/api/cancelTask")]
        [Authorize(Roles = "admin,superadmin")]
        public async Task<IActionResult> cancelTask(string tasktitle)
        {
            try 
            { 
            var task = this.getTasks().Where(p => p.Id == Guid.Parse(tasktitle)).FirstOrDefault();
            task.isCompleted = false;
            task.isActive = true;
            task.Status = 4;
            task.closedOn = DateTime.Now;
            this._applicationContext.Update(task);
            await _applicationContext.SaveChangesAsync();
            var User = this._applicationContext.Users.Where(p => p.Id == task.UserID).FirstOrDefault();
            var project = new Project()
            {
                CreatedBy = Guid.Parse(task.UserID),
                Projectname = task.TaskTitle,
                Description = task.Description,
                designPath = task.designPath
                //  ApplicationUser = User

            };
            this._applicationContext.Projects.Add(project);
            await _applicationContext.SaveChangesAsync();
            var appHelper = new AppHelper(_env);
            appHelper.saveCompletedTasks(task);
            EmailHelper emailHelper = new EmailHelper(_applicationContext, _env);
            var userEmailAddres = _applicationContext.Users.Where(p => p.Id.Equals(task.UserID)).FirstOrDefault().Email;
            var message = "Task " + task.taskNumber + " has been cancelled .";
            NotificationHub notification = new NotificationHub()
            {
                articleContent = message,
                articleHeading = "Task " + task.taskNumber,
                UserId = task.UserID,
                objectId = task.Id,
                notificationType = 1
            };

            _NotRepository.Add(notification);
            await _applicationContext.SaveChangesAsync();
            emailHelper.sendEmail(task, userEmailAddres, message);
            this._notificationProducercs.SendNotificationsToSpecificUser(task.UserID, task, message, message);
            return new OkResult();
        }
            catch(Exception ex)
            {
                _error.registerExeption(ex, _applicationContext);
                return StatusCode(500, $"Internal server error: {ex}");
             }

}


        [HttpPost("~/api/reactivateTask")]
        [Authorize(Roles = "admin,superadmin")]
        public async Task<IActionResult> reactivateTask(string taskId)
        {
            try
            {
                var task = _applicationContext.Tasks.AsNoTracking().Where(p => p.Id == Guid.Parse(taskId) && p.Projects.OrganisationID == this.organisationID).FirstOrDefault();
                task.isActive = true;
                task.Status = 2;
                task.isCompleted = false;
                var taskStatus = _applicationContext.TaskStatuses.Where(p => p.Id == 2).FirstOrDefault();
                task.Status = taskStatus.Id;
                task.TaskStatus = taskStatus;
                _applicationContext.Tasks.Update(task);
                await _applicationContext.SaveChangesAsync();
                var updatedTask = getTasks().Where(p => p.Id.Equals(Guid.Parse(taskId))).FirstOrDefault();
                return new OkObjectResult(updatedTask);
            }
            catch(Exception ex)
            {
                _error.registerExeption(ex , _applicationContext);
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        //[HttpPost("~/api/setTaskNumbeqr")]
        //public async Task<IActionResult> setTaskNumbeqr()
        //{
        //    var tasks = _applicationContext.Tasks.ToList();
        //    foreach (var t in tasks)
        //    {
        //        if (t.taskNumber == 0)
        //        {
        //            Random rn = new Random();
        //            t.taskNumber = rn.Next(1,999);
        //            await _applicationContext.SaveChangesAsync();

        //        }

        //    }
        //    return new OkResult();
        //}
        [HttpPost("~/api/addCommentToTask")]
        [Authorize]
        public async Task<IActionResult> addCommentToTask(string commnet , string taskID)
        {
            try
            {
                var task = getTasks().Where(p => p.Id == Guid.Parse(taskID)).FirstOrDefault();
                var newCommnet = new Comment()
                {
                    TaskId = task.Id,
                    tasks = task,
                    comment = "\u2022" + commnet + Environment.NewLine
                };
                EmailHelper emailHelper = new EmailHelper(_applicationContext, _env);
                var userEmail = _applicationContext.Users.Where(p => p.Id == task.UserID).FirstOrDefault().Email ;
                var message = "A new Commnet has been inserted in your task . ";
                emailHelper.sendEmail(task, userEmail, message);
                _applicationContext.Comments.Add(newCommnet);
                await _applicationContext.SaveChangesAsync();

                return new OkResult();

            }
            catch (Exception ex)
            {
                _error.registerExeption(ex , _applicationContext);
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
        [HttpPut("~/api/updateTask")]
        [Authorize]
        public async Task<IActionResult> updateTask([FromBody] Models.Task task)
        {
            try
            {
                if (task.Status == 3)
                {
                    if (task.designPath == "" || task.designPath == null)
                    {
                        return StatusCode(500, "Task is not completed");

                    }
                }

                TaskHelper taskHelper = new TaskHelper();
                var oldTask = _applicationContext.Tasks.AsNoTracking().FirstOrDefault(p => p.Id == task.Id && p.Projects.OrganisationID == this.organisationID);
                var preStatus = oldTask.Status;
                task.TypeId = 1;//Demo
                task.TaskPriority = 1;//Demo
                task.ModifiedOn = DateTime.Now.Date;
                var newStatus = task.Status;
                oldTask.Comment = task.Comment;
                var RoleAdmin = _RoleManager.Roles.Where(p => p.Name.Equals("admin") || p.Name.Equals("superadmin")).ToList();
                //var adminEmail = taskHelper.findAdmin(_applicationContext);
                var admins = new List<ApplicationUser>();
                foreach(var a in RoleAdmin)
                {
                    admins = _applicationContext.Users.Where(p => p.RoleID.Equals(a.Id)).ToList();

                }
                var adminEmail = "workalbi99@gmail.com";
                var emailBody = "";
                var user = _applicationContext.Users.Where(p => p.Id.Equals(task.UserID)).FirstOrDefault();
                string notTitle = "";
                if (preStatus != newStatus)
                {
                    if (newStatus == 2)
                    {
                         emailBody = user.FirstName + " "+ user.LastName + " has started working on Task " + task.taskNumber;
                        notTitle = user.FirstName + " " + user.LastName + " has started working on Task " + task.taskNumber;
                        //Send Email to Admin that user has completed the task
                    }
                    if (newStatus == 3)
                    {
                         
                         emailBody = user.FirstName + " "+ user.LastName + " has completed Task " + task.taskNumber;
                        notTitle = user.FirstName + " " + user.LastName + " has completed Task " + task.taskNumber;

                        //Send Email to Admin that user has completed the task
                    }
                    EmailHelper emailHelper = new EmailHelper(_applicationContext, _env);
                    NotificationHub notification = new NotificationHub()
                    {
                        articleContent = notTitle,
                        articleHeading = "Task " + task.taskNumber,
                        UserId = task.CreatedBy.ToString(),
                        objectId = task.Id,
                        notificationType = 1
                    };

                    _NotRepository.Add(notification);
                    await _applicationContext.SaveChangesAsync();
                    foreach (var admin in admins)
                    {
                        emailHelper.sendEmail(task, admin.Email, emailBody);
                        this._notificationProducercs.SendNotificationsToSpecificUser(admin.Id, task, emailBody, emailBody);
                    }
                    

                }

                this._applicationContext.Update(task);
                await this._applicationContext.SaveChangesAsync();
                return new OkObjectResult(task);

            }

            catch (Exception es)
            {
                _error.registerExeption(es , _applicationContext);
                return StatusCode(500, $"Internal server error: {es.InnerException}");
            }
        }

        [HttpPost("~/api/addDocumentToTask")]
        [Authorize]
        public async Task<IActionResult> addDocumentToTask()
        {
            try
            {
                TaskHelper taskHelper = new TaskHelper();
                var file = Request.Form.Files[0];
                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                var dbPath = await taskHelper.UploadDocument("TaskDocuments", fileName, file);
                if (!dbPath.Equals(""))
                {
                    return Ok(new { dbPath });

                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _error.registerExeption(ex , _applicationContext);
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [HttpDelete("~/api/deleteTask")]
        [Authorize(Roles = "admin,superadmin")]
        public async Task<IActionResult> deleteTask(string taskid)
        {
            try
            {
                var task = getTasks().Where(p => p.Id == Guid.Parse(taskid)).FirstOrDefault();
                var not = (from Not in _applicationContext.Notifications
                          where Not.objectId == Guid.Parse(taskid)
                          select Not).ToList();
                foreach(var n in not)
                {
                    _applicationContext.Notifications.Remove(n);

                }
                var userEmal = _applicationContext.Users.Where(p => p.Id == task.UserID).FirstOrDefault().Email;
                _applicationContext.Tasks.Remove(task);
                await _applicationContext.SaveChangesAsync();
                var userId = task.UserID;
                var message = "Your task has been removed";
                EmailHelper emailHelper = new EmailHelper(_applicationContext, _env);
                NotificationHub notification = new NotificationHub()
                {
                    articleContent = message,
                    articleHeading = "Task " + task.taskNumber,
                    UserId = task.UserID,
                    objectId = task.Id,
                    notificationType = 1
                };

                _NotRepository.Add(notification);
                await _applicationContext.SaveChangesAsync();
                await this._notificationProducercs.SendSimpleNotificationsToSpecificUser(userId, message, message);
                emailHelper.SendSimpleEmail(message , userEmal);
                return Ok();

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");

            }
        }
        [HttpPost("~/api/addDesignToTask")]
        [Authorize]
        public async Task<IActionResult> addDesignToTask()
        {
            try
            {
                TaskHelper taskHelper = new TaskHelper();
                var file = Request.Form.Files[0];
                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                var dbPath = await taskHelper.UploadDocument("Designs", fileName, file);
                if(!dbPath.Equals(""))
                {
                    return Ok(new { fileName });

                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _error.registerExeption(ex , _applicationContext);
                return StatusCode(500,ex);
            }
        }

        public IQueryable<Models.Task> getTasks()
        {
            var allTasks = _applicationContext.Tasks.Where(p => p.Projects.OrganisationID.Equals(this.organisationID));
            return allTasks;
        }

        [HttpGet("~/api/getAllTaskS")]
        [Authorize]
        public List<Models.Task> getAllTaskS()
        {
            try
            {
               
                var tasks =  this.getTasks().Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderByDescending(p => p.createdOn).ToList();
                return tasks;
            }
            catch(Exception ex)
            {
                _error.registerExeption(ex , _applicationContext);
                throw ex;
            }
        }

        public List<Subtask> GetMiniTasks(string taskId)
        {
            var subtasks = _applicationContext.Subtasks.Where(p => p.parentTaskId == Guid.Parse(taskId)).ToList();
            return subtasks;
        }

        [HttpGet("~/api/getAllMinitasks")]
        [Authorize]
        public List<Subtask> getAllMinitasks(string taskId)
        {
            try
            {
                var subtasks = this.GetMiniTasks(taskId);
                return subtasks;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("~/api/sortMiniTasks")]
        [Authorize]
        public List<Subtask> sortMiniTasks(string taskId, string dir )
        {
            try
            {
                var subtasks = this.GetMiniTasks(taskId);

                if (dir.Equals("") || dir == null)
                {
                    return subtasks;

                }
                switch (dir)
                {
                    case "asc" :
                        {
                            subtasks = subtasks.OrderBy(p => p.createdOn).ToList();
                            break;
                        }
                    case "desc":
                        {
                            subtasks = subtasks.OrderByDescending(p => p.createdOn).ToList();
                            break;
                        }

                }
                return subtasks;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpGet("~/api/getAllComments")]
        [Authorize]
        public string getAllComments(string taskId)
        {
            try
            {
                var com =  _applicationContext.Comments.Where(p => p.TaskId == Guid.Parse(taskId)).ToList();
                string comments = "";
                foreach(var c in com)
                {
                    comments = comments + c.comment;
                }
                return comments;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("~/api/sortTasks")]
        [Authorize]
        public List<Models.Task> sortTasks(string column , string dir, [FromQuery]TaskFilterDTO filters)
        {
            try
            {
                IQueryable<Models.Task> filteredTasks ;
                if (filters != null)
                {
                   filteredTasks = this.filterTasks(filters).AsQueryable();

                }
                else
                {
                    filteredTasks = getTasks();
                }
                if (column != null && dir != null)
                {
                    switch (column)
                    {
                        case "TaskName":
                            {
                                if (dir.Equals("asc"))
                                {
                                    return filteredTasks.Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderBy(p => p.taskNumber).ToList();
                                }
                                else if (dir.Equals("desc"))
                                {
                                    return filteredTasks.Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderByDescending(p => p.taskNumber).ToList();

                                }
                                break;
                            }
                        case "Title":
                            {
                                if (dir.Equals("asc"))
                                {
                                    return filteredTasks.Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderBy(p => p.TaskTitle).ToList();
                                }
                                else if (dir.Equals("desc"))
                                {
                                    return filteredTasks.Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderByDescending(p => p.TaskTitle).ToList();

                                }
                                break;
                            }
                        case "Progres":
                            {
                                if (dir.Equals("asc"))
                                {
                                    return filteredTasks.Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderBy(p => p.TaskStatus).ToList();
                                }
                                else if (dir.Equals("desc"))
                                {
                                    return filteredTasks.Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderByDescending(p => p.TaskStatus).ToList();

                                }
                                break;
                            }
                        case "createdOn":
                            {
                                if (dir.Equals("asc"))
                                {
                                    return filteredTasks.Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderBy(p => p.createdOn).ToList();
                                }
                                else if (dir.Equals("desc"))
                                {
                                    return filteredTasks.Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderByDescending(p => p.createdOn).ToList();

                                }
                                break;
                            }
                        case "deadline":
                            {
                                if (dir.Equals("asc"))
                                {
                                    return filteredTasks.Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderBy(p => p.Deadline).ToList();
                                }
                                else if (dir.Equals("desc"))
                                {
                                    return filteredTasks.Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderByDescending(p => p.Deadline).ToList();

                                }
                                break;
                            }
                        default:
                            {
                                return filteredTasks.Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderByDescending(p => p.createdOn).ToList();
                            }
                    }
                    return filteredTasks.Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderByDescending(p => p.createdOn).ToList();

                }
                else
                
                return filteredTasks.Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderByDescending(p => p.createdOn).ToList();

            }
            catch (Exception ex)
            {
                _error.registerExeption(ex, _applicationContext);
                throw ex;
            }
        }


        [HttpGet("~/api/getTaskById")]
        [Authorize]
        public List<Models.Task> getTaskById(string id)
        {
            try
            {
                return getTasks().Where(p => p.Id == Guid.Parse(id)).Include(c => c.User).Include(p => p.Projects).ToList();
            }
            catch (Exception ex)
            {
                _error.registerExeption(ex, _applicationContext);
                throw ex;
            }
        }


        [HttpGet("~/api/getUserTasks")]
        [Authorize]
        public List<Models.Task> getUserTasks(string userID)
        {
            try
            {
                var tasks = getTasks().Where(p => p.UserID == userID && p.isActive == true).Include(p => p.TaskStatus).OrderByDescending(p => p.createdOn).ToList();
                return tasks;
                    
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
      
        [HttpGet("~/api/getTasksbyTitle")]
        [Authorize]
        public List<Models.Task> getTasksbyTitle(string title)
        {
            try
            {
                var tasks = getTasks().Where(p => p.TaskTitle.Contains(title)).Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderByDescending(p => p.createdOn).ToList();
                return tasks;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet("~/api/getTasksByProject")]
        [Authorize]
        public List<Models.Task> getTasksByProject(string ProjetcID)
        {
            try
            {
                return getTasks().Where(p => p.ProjectId == Guid.Parse(ProjetcID)).Include(c => c.User).Include(p => p.Projects).OrderByDescending(p => p.createdOn).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("~/api/getActiveTsks")]
        [Authorize]
        public List<Models.Task> getActiveTsks()
        {
            try
            {
                var tasks = getTasks().Where(p => p.Status == 1 || p.Status == 2).Include(c => c.User).Include(p => p.Projects).OrderByDescending(p => p.createdOn).ToList();
                return tasks;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //[HttpGet("~/api/getTaskByStatus")]
        //[Authorize]
        public List<Models.Task> getTaskByStatus(int[] status)
        {
            try
            {
                if (status == null)
                {
                    return this.getTasks().Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderByDescending(p => p.createdOn).ToList();

                }
                List<int> statusArray = new List<int>();
                List<Models.Task> tasks = new List<Models.Task>();
                if (status.Length > 1)
                {
              
                    foreach (var elementStatus in status)
                    {
                        var task = getTasks().Where(p => p.Status == (int)elementStatus).Include(c => c.User).Include(p => p.Projects).OrderByDescending(p => p.createdOn).ToList();
                        if (tasks.Count == 0)
                        {
                            tasks = task;
                        }
                        else
                        {
                            tasks = tasks.Concat(task).ToList();

                        }
                    }
                    return tasks;

                }
                else
                {
                    return getTasks().Where(p => p.Status == status.FirstOrDefault()).Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderByDescending(p => p.createdOn).ToList();

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        //[HttpGet("~/api/getTasksByUser")]
        //[Authorize]
        public List<Models.Task> getTasksByUser(string userIds)
        {
            try
            {
                if (userIds == null)
                {
                    return getTasks().Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).ToList();
                }
                else
                {
                    string[] userIdsArray = { };
                    List<Models.Task> tasks = new List<Models.Task>();
                    if (userIds.Contains(","))
                    {
                        userIdsArray = userIds.Split(',');
                        foreach (var userID in userIdsArray)
                        {
                            var task = getTasks().Where(p => p.UserID.Equals(userID)).Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).ToList();
                            if (tasks.Count == 0)
                            {
                                tasks = task;
                            }
                            else
                            {
                                tasks = tasks.Concat(task).ToList();

                            }
                        }
                    }
                    else
                    {
                        tasks = getTasks().Where(p => p.UserID.Equals(userIds)).Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderByDescending(p => p.createdOn).ToList();

                    }

                    return tasks.ToList();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet("~/api/filterTasks")]
        [Authorize]
        public List<Models.Task> filterTasks([FromQuery]TaskFilterDTO filter)
        {
            try
            {
                if(filter == null)
                {
                    return this.getTasks().Include(c => c.User).Include(p => p.Projects).Include(p => p.TaskStatus).OrderByDescending(p => p.createdOn).ToList();
                }
                var tasksFilteruser = new List<Models.Task>();
                if(filter.userIDs != null  )
                {
                    var s = String.Join(",", filter.userIDs);

                    tasksFilteruser = this.getTasksByUser(s);
                }
                var tasksFilterStatus = new List<Models.Task>();
                if(filter.status != null)
                {

                     tasksFilterStatus = this.getTaskByStatus(filter.status);
                }
                //if(tasksFilterStatus.Count ==0)
                //{
                //    return tasksFilteruser;
                //}
                //if (tasksFilteruser.Count == 0)
                //{
                //    return tasksFilterStatus;
                //}
                if(filter.status != null && filter.userIDs != null)
                {
                    var filteredtasks = from fTu in tasksFilteruser
                                        join fts in tasksFilterStatus
                                        on fTu.Id equals fts.Id
                                        select fTu;
                    var t = filteredtasks.ToList();
                    return t;
                }
                else if(filter.userIDs != null)
                    {
                    return tasksFilteruser;
                }
                else if(filter.status != null)
                {
                    return tasksFilterStatus;
                }
                return null;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
