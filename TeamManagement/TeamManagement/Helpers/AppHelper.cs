using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using TeamMan.Models;
using Task = TeamMan.Models.Task;

namespace TeamMan.Helpers
{
    public class AppHelper
    {
        private readonly IHostingEnvironment _env;
        public AppHelper( IHostingEnvironment env)
        {
            _env = env;

        }
        public  bool IsZipValid(string path)
        {
            try
            {
                var pathFile = _env.ContentRootPath + path;
                using (var zipFile = ZipFile.OpenRead(pathFile))
                {
                    var entries = zipFile.Entries;
                    return true;
                }
            }
            catch (InvalidDataException ex)
            {
                return false;
            }
        }
        public string saveCompletedTasks(Task task)
        {
            try
            {
              
                    // add this map file into the "images" directory in the zip archive
                    // add the report into a different directory in the archive
                    var filename = _env.ContentRootPath + "\\Assets\\Designs\\" + task.designPath;
                    string completedTaskPath = _env.ContentRootPath +  "\\Assets\\CompletedTasks\\Task" + task.taskNumber + ".rar";
                    System.IO.File.Copy(filename, completedTaskPath);

                    //bool isVaid = IsZipValid(filename);
                    //if(isVaid)
                    //{
                    //    zip.Save("Assets\\CompletedTasks\\Task" + task.taskNumber);
                    //}
                    //else
                    //{
                    //    File.Delete(filename);
                    //    throw new Exception("File you entered is not valid");
                    //}
                    return "\\Assets\\CompletedTasks\\Task" + task.taskNumber +".rar";
                
                
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }
    }
}
