using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamMan.Models;

namespace TeamMan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private ApplicationContext _applicationContext;
        private readonly string loggedUserID;
        private readonly Guid organisationID;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ProjectController(ApplicationContext applicationContext , IHttpContextAccessor httpContextAccessor )
        {
            _applicationContext = applicationContext;
            _httpContextAccessor = httpContextAccessor;
            this.loggedUserID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name).Value;
            var loggedUser = _applicationContext.Users.Where(p => p.Id.Equals(this.loggedUserID)).FirstOrDefault();
            organisationID = loggedUser.OrganisationID;
        }

        [HttpGet("~/api/getAllProjects")]
        [Authorize]
        public List<Models.Project> getAllProjects()
        {
            try
            {
           
                return _applicationContext.Projects.Where(p => p.OrganisationID == organisationID)
                    .Include(p => p.Organisation)
                    .Include(p => p.Client)
                    .Include(p => p.ProjectManager)
                    .OrderByDescending(p => p.createdOn).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
