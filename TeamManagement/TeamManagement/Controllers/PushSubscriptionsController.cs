using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lib.Net.Http.WebPush;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeamMan.Interfaces;

namespace TeamMan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PushSubscriptionsController : ControllerBase
    {
        private readonly IPushSubscriptionsService _pushSubscriptionsService;

        public PushSubscriptionsController(IPushSubscriptionsService pushSubscriptionsService)
        {
            _pushSubscriptionsService = pushSubscriptionsService;
        }

        [HttpPost]
        public IActionResult Post([FromBody] PushSubscription subscription)
        {
            _pushSubscriptionsService.Insert(subscription);
            return new OkObjectResult(subscription);
        }

        [HttpDelete("{endpoint}")]
        public void Delete(string endpoint)
        {
            _pushSubscriptionsService.Delete(endpoint);
        }
    }
}
