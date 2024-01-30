using Lib.Net.Http.WebPush;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamMan.Interfaces;

namespace TeamMan.Services
{
    public class PushSubscriptionsService : IPushSubscriptionsService , IDisposable
    {
        private readonly LiteDatabase _db;
        private readonly LiteCollection<PushSubscription> _collection;

        public PushSubscriptionsService()
        {
            _db = new LiteDatabase("PushSubscriptionsStore.db");
            _collection = (LiteCollection<PushSubscription>)_db.GetCollection<PushSubscription>("subscriptions");
        }

        public LiteCollection<PushSubscription> GetAll()
        {
            return _collection;
        }

        public void Insert(PushSubscription subscription)
        {
            _collection.Insert(subscription);
        }

        public void Delete(string endpoint)
        {
            _collection.Delete(endpoint);
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
