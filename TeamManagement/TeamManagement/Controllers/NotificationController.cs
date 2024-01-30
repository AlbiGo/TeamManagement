using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Logging;
using TeamMan.Interfaces;
using TeamMan.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TeamMan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        // GET: api/<NotificationController>
        private readonly ApplicationContext _applicationContext;
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string loggedUserID;
        public LogHelper _error = new LogHelper();


        public NotificationController(ApplicationContext applicationContext,
            INotificationService notificationService,
            IHttpContextAccessor httpContextAccessor      
            )
        {
            _applicationContext = applicationContext;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            this.loggedUserID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name).Value;

        }
        [HttpGet("~/api/GetNotificationHubsByUser")]
        public List<NotificationHub> GetNotificationHubsByUser()
        {
            var notifications =  _notificationService.GetNotificationHubsByUser(loggedUserID);
            return notifications;
        }
        [HttpGet("~/api/getAllNot")]
        public List<NotificationHub> getAllNot()
        {
            var notifications = _notificationService.getAllNotifications(loggedUserID);
            return notifications;
        }
        // GET api/<NotificationController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<NotificationController>
        [HttpPost("~/api/readNotifications")]
        public async Task<IActionResult> readNotifications()
        {
            try
            {
                await _notificationService.openNotifications(loggedUserID);
                return Ok();

            }
            catch (Log ex)
            {
              //  _error.registerExeption(ex , _applicationContext);
                return StatusCode(500, $"Internal server error: {ex}");

            }
        }
        [HttpPost("~/api/openNotification")]
        public async Task<IActionResult> openNotification( string id)
        {
            try
            {
                await _notificationService.readNotification(loggedUserID , id);
                return Ok();

            }
            catch (Log ex)
            {
                //  _error.registerExeption(ex , _applicationContext);
                return StatusCode(500, $"Internal server error: {ex}");

            }
        }

        // PUT api/<NotificationController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<NotificationController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
