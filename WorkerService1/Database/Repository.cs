using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService1.Database
{
    public class Repository :IRepository
    {
        private readonly UserRg_DbContext _db;
        public Repository(UserRg_DbContext db)
        {
            _db = db;
        }

        public bool SaveChanges()
        {
            return _db.SaveChanges() > 0;

        }
        public T Get<T>(Expression<Func<T, bool>> filters) where T : class
        {
            return _db.Set<T>().Where(filters).FirstOrDefault();
        }

        public T Get<T>(Expression<Func<T, bool>> filters, params Expression<Func<T, object>>[] includeProperties) where T : class
        {
            var data = _db.Set<T>().Where(filters);
            foreach (var item in includeProperties)
                data.Include(item).Load();


            return data.FirstOrDefault();
        }
        public IEnumerable<T> GetAll<T>() where T : class
        {

            return _db.Set<T>().AsNoTracking().ToList();
        }

        public IEnumerable<T> GetAll<T>(Expression<Func<T, bool>> filters) where T : class
        {
            return _db.Set<T>().Where(filters).AsNoTracking().ToList();
        }


        public IEnumerable<T> GetAll<T>(Expression<Func<T, bool>> filters, params Expression<Func<T, object>>[] includeProperties) where T : class
        {
            var data = _db.Set<T>().Where(filters);
            foreach (var item in includeProperties)
                data.Include(item).Load();


            return data.ToList();
        }

        public IEnumerable<T> GetAll<T>(params Expression<Func<T, object>>[] includeProperties) where T : class
        {
            var data = _db.Set<T>();
            foreach (var item in includeProperties)
                data.Include(item).Load();


            return data.ToList();
        }
        public void Update<T>(T entity) where T : class
        {
            _db.Entry(entity).State = EntityState.Modified;
        }

    }
}
