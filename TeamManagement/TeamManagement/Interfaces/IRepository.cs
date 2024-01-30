using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.Interfaces
{
    public interface IRepository<T> where T : class
    {
        public void Add(T entity);
        public void Update(T entity);
        public void Delete(T entity);
        public T Find(T entity);
        public Task<T> SaveAsync(T entity);
    }
}
