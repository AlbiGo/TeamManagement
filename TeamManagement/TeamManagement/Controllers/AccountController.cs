using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TeamMan.Helpers;
using TeamMan.Interfaces;
using TeamMan.Models;
using TeamMan.Services;
using TeamMan.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Data;
//using Microsoft.AspNetCore.Cors;
using System.Web.Http.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace TeamMan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[EnableCors("EnableCORS")]
    //[EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AccountController : ControllerBase
    {
        private ApplicationContext _applicationContext;
        private UserInterface<ApplicationUser> _userInterface;
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IHostingEnvironment _env;
        private IRepository<ApplicationUser> _repository;
        private IRepository<NotificationHub> _repositoryNot;
        private readonly INotificationProducercs _notificationProducercs;
        private readonly string loggedUserID;
        private readonly IConfiguration _Configuration;
        private readonly Guid organisationID;
        public AccountController( ApplicationContext applicationContext, IConfiguration Configuration,
            INotificationProducercs notificationProducercs, IRepository<NotificationHub> repositoryNot,
            IRepository<ApplicationUser> repository, IHostingEnvironment env, IHttpContextAccessor httpContextAccessor, UserInterface<ApplicationUser> userInterface , UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signManager )
        {
            this._applicationContext = applicationContext;
            _userInterface = userInterface;
            _userManager = userManager;
            _signManager = signManager;
            _httpContextAccessor = httpContextAccessor;
            _env = env;
            _repository = repository;
            _notificationProducercs = notificationProducercs;
            _Configuration = Configuration;

            if (_httpContextAccessor.HttpContext.User.Claims.Count() > 0)
            {
                this.loggedUserID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name).Value;
                var loggedUser = _applicationContext.Users.Where(p => p.Id.Equals(this.loggedUserID)).FirstOrDefault();
                organisationID = loggedUser.OrganisationID;
            }
            _repositoryNot = repositoryNot;
        }
        [HttpPost("~/api/Login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            AuthHelper _authHelper = new AuthHelper(_Configuration);
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await this._userInterface.LogIn(model);
                    if (result.Succeeded)
                    {
                        var user = await _userManager.FindByNameAsync(model.username);
                        var claims = await _userManager.GetClaimsAsync(user);
                        string tokenString = await _authHelper.generateToken(model, claims , Guid.Parse(user.Id), _applicationContext);
                        return Ok(new { Token = tokenString });
                    }
                    else
                    {
                        return  Unauthorized();
                    }
                }
                return new BadRequestObjectResult(ModelState);
            }
           catch(Exception ex)
            {
                return StatusCode(500,ex.Message);
            }


        }
        [HttpPost("~/api/createUser")]
        [Authorize(Roles = "superadmin")]
        public async Task<IActionResult> createUser(RegistrationViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var createdUser = await this._userInterface.RegisterUser(model);
                    if (createdUser != null)
                    {
                        EmailHelper emailHelper = new EmailHelper(_applicationContext, _env);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(createdUser);
                        var callbackUrl = Url.Action(nameof(VerifyEmail), "/api", new { userId = createdUser.Id, code }, Request.Scheme, Request.Host.ToString());
                        string Message = "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>";
                        //Send email to created User 
                        //Addition : include credentials on email body
                        string subject = "You account has been activated on Team Management.";
                        emailHelper.sendEmailOnCreateUser(subject, model, Message, callbackUrl);
                        return new OkResult();
                    }
                    else
                    {
                        return new BadRequestResult();
                    }
                }
                return new BadRequestObjectResult(ModelState);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
        [HttpPost("~/api/Register")]
        public async Task<IActionResult> Register(SignUpDTO model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await this._userInterface.SignUp(model);
                    if (result != null)
                    {
                        EmailHelper emailHelper = new EmailHelper(_applicationContext, _env);
                        var createdUser = _userManager.Users.Where(p => p.UserName.Equals(result[0])).FirstOrDefault();
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(createdUser);
                        var callbackUrl = Url.Action(nameof(VerifyEmail), "/api", new { userId = createdUser.Id, code }, Request.Scheme, Request.Host.ToString());
                        string Message = "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>";
                        //Send email to created User 
                        //Addition : include credentials on email body
                        string subject = "You account has been activated on Team Management.";
                        emailHelper.sendEmailToSignedUpUser(subject, Message, callbackUrl, result[2],result[0],result[1]);
                        return new OkResult();
                    }
                    else
                    {
                        return new BadRequestResult();
                    }
                }
                return new BadRequestObjectResult(ModelState);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
        [HttpGet("~/api/VerifyEmail")]
        public async Task<IActionResult> VerifyEmail(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return BadRequest();
            }

            if (user.EmailConfirmed)
            {
                return Ok();
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                user.isActive = true;
                await _applicationContext.SaveChangesAsync();
                return Ok();
            }

            return BadRequest();
        }

        [HttpPut("~/api/UpdateUser")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(ApplicationUser model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByIdAsync(model.Id);
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Address = model.Address;
                    user.Email = model.Email;
                    user.Description = model.Description;
                    user.Description = model.Description;
                    await _applicationContext.SaveChangesAsync();

                    return new OkResult();

                }
                else
                {
                    return new BadRequestResult();
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");

            }
        }
        [HttpPost("~/api/DeactivateUser")]
        [Authorize]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            try
            {
               await this._userInterface.DeactivateUser(id);//Send Email
                string message = "Your user has been deactivated";
                string titlle = "Your user has been deactivated";
                await this.Sendnotification(titlle, id, message);
                return new OkResult();

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");

            }
        }

        [HttpDelete("~/api/Deleteuser")]
        [Authorize]
        public async Task<IActionResult> Deleteuser(string id)
        {
            try
            {
                string message = "Your user has been deleted";
                string titlle = "Your user has been deleted";
                await this.Sendnotification( titlle,id,message);
                await this._userInterface.Delete(id);//Send email
                return new OkResult();

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");

            }
        }

        [HttpGet("~/api/getAllUsers")]
        [Authorize] 
        public  List<ApplicationUser> getAllUsers()
        {
            try
            {
                
                return this._userInterface.getAllUsers(this.organisationID.ToString());

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet("~/api/getAllActiveUsers")]
        [Authorize]
        public List<ApplicationUser> getAllActiveUsers()
        {
            try
            {

                return this._userInterface.getAllUsers(this.organisationID.ToString()).Where(p => p.EmailConfirmed == true && p.isActive == true).ToList();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("~/api/getUserInfo")]
        [Authorize]
        public async Task<ApplicationUser> getUserInfo(string id)
        {
            try
            {
                return await this._userInterface.getUserDetails(id);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("~/api/getUserById")]
        [Authorize]
        public async Task<ApplicationUser> getUserById(string id)
        {
            try
            {
                return await this._userInterface.getUserDetails(id);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost("~/api/ChangePassord")]
        [Authorize]
        public async Task<IActionResult> ChangePassord(ChangePasswordDTO model)
        {
            try
            {
                 await this._userInterface.changeUserPassword(model);
                return Ok();

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        public async System.Threading.Tasks.Task Sendnotification(string title, string userId, string content)
        {
            try
            {
                string titleNot = title;
                string messageNot = content;
                // emailHelper.sendEmail(task, userToSend.Email, messageNot);
                NotificationHub notification = new NotificationHub()
                {
                    articleContent = messageNot,
                    articleHeading = titleNot,
                    UserId = userId,
                    notificationType = 3
                };

                _repositoryNot.Add(notification);
                await _applicationContext.SaveChangesAsync();
                await this._notificationProducercs.SendSimpleNotificationsToSpecificUser(userId, messageNot, titleNot);
            }
            catch(Exception ex)
            {
                throw ex;
            }
           
        }
    }
   
}