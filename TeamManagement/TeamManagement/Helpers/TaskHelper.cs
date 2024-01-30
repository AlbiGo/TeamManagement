using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TeamMan.Models;

namespace TeamMan.Helpers
{
    public class TaskHelper
    {
        public string findAdmin(ApplicationContext context)
        {
            var adminId = context.TeamRoles.Where(p => p.Name == "TeamLeader").FirstOrDefault().Id;
            var adminEmail = context.Users.Where(p => p.TeamRoleID == adminId).FirstOrDefault().Email;
            return adminEmail;
        }


        public async Task<string> UploadDocument(string foldername, string fileName, IFormFile file)
        {
            var folderName = Path.Combine("Assets", fileName);
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

            if (file.Length > 0)
            {
                folderName = Path.Combine("Assets", foldername);
                pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                var fullPath = Path.Combine(pathToSave, fileName);
                var dbPath = Path.Combine(folderName, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return dbPath;

            }
            else
                return "";

        }

    }
   
}
