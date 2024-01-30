using Lib.Net.Http.WebPush;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Models
{
    public class AngularPushNotification
    {
        public class NotificationAction : Hub
        {
            public string Action { get; }
            public string Title { get; }
            public string userId { get; set; }


            public NotificationAction(string action, string title)
            {
                Action = action;
                Title = title;
            }
        }

        public string Title { get; set; }
        public string Body { get; set; }
        public int? TimeToLive { get; set; }
        private const string WRAPPER_START = "{\"notification\":";
        private const string WRAPPER_END = "}";
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public PushMessage ToPushMessage(string topic = null, int? timeToLive = null, PushMessageUrgency urgency = PushMessageUrgency.Normal)
        {
            return new PushMessage(WRAPPER_START + JsonConvert.SerializeObject(this, _jsonSerializerSettings) + WRAPPER_END)
            {
                Topic = topic,
                TimeToLive = timeToLive,
                Urgency = urgency
            };
        }
    }
}
