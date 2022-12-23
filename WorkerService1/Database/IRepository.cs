using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService1.Database
{
    public interface IRepository
    {
        bool SaveChanges();
        IEnumerable<T> GetAll<T>() where T : class;
        IEnumerable<T> GetAll<T>(Expression<Func<T, bool>> filters) where T : class;
        IEnumerable<T> GetAll<T>(Expression<Func<T, bool>> filters, params Expression<Func<T, object>>[] includeProperties) where T : class;
        IEnumerable<T> GetAll<T>(params Expression<Func<T, object>>[] includeProperties) where T : class;

        T Get<T>(Expression<Func<T, bool>> filters) where T : class;
        T Get<T>(Expression<Func<T, bool>> filters, params Expression<Func<T, object>>[] includeProperties) where T : class;

        void Update<T>(T entity) where T : class;

    }
}
