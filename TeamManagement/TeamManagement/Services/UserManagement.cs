using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TeamMan.Helpers;
using TeamMan.Interfaces;
using TeamMan.Models;
using TeamMan.ViewModels;

namespace TeamMan.Services
{
    public class UserManagement<T> : UserInterface<T> where T : class
    {

        private ApplicationContext _context;
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signManager;
        private RoleManager<IdentityRole> _RoleManager;
        private IHostingEnvironment _env;
        private IRepository<ApplicationUser> _repository;

        public UserManagement(ApplicationContext context, IRepository<ApplicationUser> repository, IHostingEnvironment env, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _signManager = signManager;
            _RoleManager = roleManager;
            _env = env;
            _repository = repository;
        }

        public async Task<ApplicationUser> RegisterUser(RegistrationViewModel model)
        {
            try
            {
                EmailHelper emailHelper = new EmailHelper(_context, _env);
                var teamRole = _context.TeamRoles.Where(p => p.Id == Guid.Parse(model.teamRoleId)).FirstOrDefault();
                IdentityRole userROLe = _RoleManager.Roles.Where(p => p.Id == model.teamRoleId).FirstOrDefault();
                string userName = model.FirstName + model.LastName;
                var createdBy = model.createdBy;
                var createdByuser = _context.Users.Where(p => p.Id.Equals(createdBy)).FirstOrDefault();
                var company = _context.Organisations.Where(p => p.Id == createdByuser.OrganisationID).FirstOrDefault();
                var newUser = new ApplicationUser()
                {
                    UserName = userName,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.phoneNumber,
                    Address = model.Address,
                    Email = model.Email,
                    TeamId = model.team,
                    DepartmentID = model.department,
                    RoleID = userROLe.Id,
                    TeamRoleID = new Guid(model.teamRoleId),
                    profileUrl = @"Assets\UserProfilePics\0.jfif",
                    TeamRoles = teamRole,
                    Organisation = company,
                    OrganisationID = company.Id,
                    isActive = true,
                    EmailConfirmed = true


                };
                newUser.Roles = userROLe;
                IdentityResult result = await _userManager.CreateAsync(newUser, model.password);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(newUser.UserName);
                    IList<Claim> claims = await _RoleManager.GetClaimsAsync(userROLe);
                    await _userManager.AddClaimAsync(newUser, claims.FirstOrDefault());
                    await _context.SaveChangesAsync();
                    return user;
                }
                else
                {
                    return null;
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<string>> SignUp(SignUpDTO model)
        {
            try 
            { 

                EmailHelper emailHelper = new EmailHelper(_context, _env);
                var teamRole = _context.TeamRoles.Where(p => p.Name.Equals("superadmin")).FirstOrDefault();
                IdentityRole userROLe = _RoleManager.Roles.Where(p => p.Id == teamRole.roleId).FirstOrDefault();
                string userName = model.FirstName + model.LastName;
                var company = model.Company;
                var newCompany = new Organisation()
                {
                    OrganisationName = company,
                    Address = model.Address
                };
                _context.Organisations.Add(newCompany);
                await _context.SaveChangesAsync();
                var addedCompany = _context.Organisations.Where(p => p.OrganisationName.Equals(model.Company)).FirstOrDefault();
                var rep = new List<string>();
                var newUser = new ApplicationUser()
                {
                    UserName = userName,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    Email = model.Email,
                    RoleID = userROLe.Id,
                    TeamRoleID = teamRole.Id,
                    isDemo = true,
                    //profileUrl = @"Assets\UserProfilePics\0.jfif",
                    EmailConfirmed = true,
                    Organisation = addedCompany,
                    OrganisationID = addedCompany.Id


                };
                newUser.Roles = userROLe;
                var password = this.GeneratePassword();
                IdentityResult result = await _userManager.CreateAsync(newUser, model.password);
                if (result.Succeeded)
                {
               
                    var user = await _userManager.FindByNameAsync(newUser.UserName);
                    IList<Claim> claims = await _RoleManager.GetClaimsAsync(userROLe);
                    await _userManager.AddClaimAsync(newUser, claims.FirstOrDefault());
                    await _context.SaveChangesAsync();
                    rep.Add(user.UserName);
                    rep.Add(user.Email);
                    rep.Add(password);
                    return rep;
                }
                else
                {
                    string error = string.Join("," , result.Errors);
                    throw new Exception(error);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

}
        public string GeneratePassword()
        {
            var options = _userManager.Options.Password;

            int length = options.RequiredLength;

            bool nonAlphanumeric = options.RequireNonAlphanumeric;
            bool digit = options.RequireDigit;
            bool lowercase = options.RequireLowercase;
            bool uppercase = options.RequireUppercase;

            StringBuilder password = new StringBuilder();
            Random random = new Random();

            while (password.Length < length)
            {
                char c = (char)random.Next(32, 126);

                password.Append(c);

                if (char.IsDigit(c))
                    digit = false;
                else if (char.IsLower(c))
                    lowercase = false;
                else if (char.IsUpper(c))
                    uppercase = false;
                else if (!char.IsLetterOrDigit(c))
                    nonAlphanumeric = false;
            }

            if (nonAlphanumeric)
                password.Append((char)random.Next(33, 48));
            if (digit)
                password.Append((char)random.Next(48, 58));
            if (lowercase)
                password.Append((char)random.Next(97, 123));
            if (uppercase)
                password.Append((char)random.Next(65, 91));

            return password.ToString();
        }
        public async System.Threading.Tasks.Task UpdateUser(ApplicationUser model)
        {
            try
            {
                this._repository.Update(model);
                await _context.SaveChangesAsync();
                
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async System.Threading.Tasks.Task DeactivateUser(string id)
        {
            try
            {
                var user = await this._userManager.FindByIdAsync(id);
                var tasks = _context.Tasks.Where(p => p.UserID.Equals(id)).ToList();
                foreach (var t in tasks)
                {
                    t.isActive = false ;
                }
                user.isActive = false;
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public async System.Threading.Tasks.Task Delete(string id)
        {
            try {
                var tasks = _context.Tasks.Where(p => p.UserID.Equals(id)).ToList();
                foreach(var t in tasks)
                {
                    _context.Tasks.Remove(t);
                }
                var notifications = _context.Notifications.Where(p => p.UserId.Equals(id)).ToList();
                foreach (var n in notifications)
                {
                    _context.Notifications.Remove(n);
                }

                var user =  this._context.Users.Where(p => p.Id.Equals(id)).FirstOrDefault();
                await this._userManager.DeleteAsync(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public ApplicationUser getTeamAdmin()
        {
            try
            {
                IdentityRole userROLe = _RoleManager.Roles.Where(p => p.Name == "admin").FirstOrDefault();
                var roleId = userROLe.Id;
                var admin = (from users in _context.Users
                            where users.RoleID == roleId
                            select users).FirstOrDefault();
                return admin;
            }
            catch (Log ex)
            {
                throw ex;
            }
        }
        public ApplicationUser getTeamLeader()
        {
            try
            {
                var teamLeader = (from users in _context.Users
                                  where users.TeamRoleID == new Guid("988C3254-DF32-4159-AF1B-08D83D32805B")
                                  select users).FirstOrDefault();
                return teamLeader;
            }
            catch (Log ex)
            {
                throw ex;
            }
        }



        public async Task<Microsoft.AspNetCore.Identity.SignInResult> LogIn(LoginViewModel model)
        {
            try
            {
                var result = new Microsoft.AspNetCore.Identity.SignInResult();
                ApplicationUser user = await _userManager.FindByNameAsync(model.username);
                if (user != null)
                {
                    var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
                    if(emailConfirmed == false)
                    {
                        throw new Exception("Please avtivate your email");
                    }
                    else
                    {
                        if(user.isActive == false)
                        {
                            throw new Exception("Your account is not activated.");

                        }
                        result = await _signManager.PasswordSignInAsync(model.username, model.password, model.RememberMe, false);
                    }
                }
                else
                {
                    return Microsoft.AspNetCore.Identity.SignInResult.Failed;
                }
                return result;
            }

            catch(Exception ex)
            {
                throw ex;
            }

        }


        public  List<ApplicationUser> getAllUsers(string orgID)
        {
            return  this._context.Users.Include(p => p.TeamRoles).Where(p => p.OrganisationID == Guid.Parse(orgID)).ToList();
        }
        
        public async Task<ApplicationUser> getUserDetails(string id)
        {
            try
            {
                //Check if User Exists
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    return user;
                }
                else return null;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async System.Threading.Tasks.Task changeUserPassword(ChangePasswordDTO model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.userId);
                if(user == null)
                {
                    throw new Exception("User does not exist.");
                }
                var newPassword = _userManager.PasswordHasher.HashPassword(user, model.password);
                user.PasswordHash = newPassword;
                var res = await _userManager.UpdateAsync(user);
            }
        
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

}
