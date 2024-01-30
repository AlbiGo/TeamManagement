using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamMan.Models;
using TeamMan.ViewModels;

namespace TeamMan.Interfaces
{
    public interface UserInterface<T> where T : class
    {
        public Task<ApplicationUser> RegisterUser(RegistrationViewModel model);
        public Task<Microsoft.AspNetCore.Identity.SignInResult> LogIn(LoginViewModel model);
        public  System.Threading.Tasks.Task UpdateUser(ApplicationUser model);

        public List<ApplicationUser> getAllUsers(string s);
        public Task<ApplicationUser> getUserDetails(string username);
        public ApplicationUser getTeamAdmin();
        public ApplicationUser getTeamLeader();

        public System.Threading.Tasks.Task changeUserPassword(ChangePasswordDTO model);

        public  System.Threading.Tasks.Task DeactivateUser(string id);
        public System.Threading.Tasks.Task Delete(string id);
        public  Task<List<string>> SignUp(SignUpDTO model);


    }
}
