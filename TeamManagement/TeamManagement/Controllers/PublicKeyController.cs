using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using static TeamMan.Startup;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TeamMan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicKeyController : ControllerBase
    {
        private readonly Startup.PushNotificationsOptions _options;
        
        public PublicKeyController(IOptions<PushNotificationsOptions> options)
        {
            _options = options.Value;
        }

        public ContentResult Get()
        {
            return Content(_options.PublicKey, "text/plain");
        }
    }
}
