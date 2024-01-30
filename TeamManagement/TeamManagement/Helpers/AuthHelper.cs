using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TeamMan.Models;
using TeamMan.ViewModels;

namespace TeamMan.Helpers
{
    public class AuthHelper
    {


        private readonly IConfiguration _Configuration;
        public AuthHelper(IConfiguration Configuration)
         {
            _Configuration = Configuration;
        }
        public async Task<string> generateToken(LoginViewModel user , IList<Claim> Claims , Guid loggegUser , ApplicationContext _applicationContext)
        {
            var Key = _Configuration["AppSettings:Secret"];
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var userOr = _applicationContext.Users.Where(p => p.Id.Equals(loggegUser.ToString())).FirstOrDefault().OrganisationID;
            var organisationID = _applicationContext.Organisations.Where(p => p.Id.Equals(userOr)).FirstOrDefault().Id;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, loggegUser.ToString()),
            new Claim(ClaimTypes.Role, Claims[0].Value) ,
            new Claim(ClaimTypes.PrimaryGroupSid , organisationID.ToString())
        };

            var tokeOptions = new JwtSecurityToken(
                issuer: "https://localhost:44342",
                audience: "https://localhost:44342",
                claims : claims,
                expires: DateTime.Now.AddDays(10),
                signingCredentials: signinCredentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            //var userID = loggegUser;
            //IdentityUserToken<string> u = new IdentityUserToken<string>()
            //{
            //    Name = "User Login",
            //    Value = tokenString,
            //    UserId = userID.ToString(),
            //    LoginProvider = "App"

            //};

            //_applicationContext.UserTokens.Add(u);
            //await _applicationContext.SaveChangesAsync();
            return tokenString;
        }
    }
}
