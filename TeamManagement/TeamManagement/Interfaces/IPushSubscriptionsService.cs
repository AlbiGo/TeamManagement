using Lib.Net.Http.WebPush;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Interfaces
{
    public interface IPushSubscriptionsService 
    {
        public LiteCollection<PushSubscription> GetAll();
        public void Insert(PushSubscription subscription);
        public void Delete(string endpoint);
        public void Dispose();


    }
}
