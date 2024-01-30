using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamMan.Interfaces;
using TeamMan.Models;

namespace TeamMan.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationContext _context;

        public Repository(ApplicationContext context)
        {
            _context = context;
        }

        public void Add(T entity)
        {
            _context.Set<T>().Add(entity);
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public T Find(T entity)
        {
            return _context.Set<T>().Find(entity);
        }
        public async Task<T> SaveAsync(T entity)
        {
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}
