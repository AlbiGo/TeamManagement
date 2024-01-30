using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamMan.Models;

namespace TeamMan.Helpers
{
    public class LogHelper
    {
        public void registerExeption(Exception ex, ApplicationContext _applicationContext)
        {
            Log log = new Log()
            {
                LogMessage = ex.Message,
                Description = ex.StackTrace
            };
            _applicationContext.Logs.Add(log);
            _applicationContext.SaveChanges();


        }
    }
}
