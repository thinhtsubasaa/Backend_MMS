
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ERP.Infrastructure
{
    public interface IRepository<T> where T : class
    {
        // Marks an entity as new
        T Add (T entity);
        void AddRange(IEnumerable<T> entities);
        // Marks an entity as modified
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        // Marks an entity to be removed
        void Delete(object id);
        void DeleteRange(List<object> lst);
        void RemoveRange(IEnumerable<T> entities);
        // Get an entity by object id
        T GetById(object id);
        T GetSingle(Expression<Func<T, bool>> whereCondition, string[] includes = null);
        ICollection<T> GetAll_No_Condition(Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string[] includes = null);
        ICollection<T> GetAll(Expression<Func<T, bool>> whereCondition, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string[] includes = null);
        ICollection<T> GetAllPaging(Expression<Func<T, bool>> whereCondition, out int total, int page, int pageSize, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string[] includes = null);
        ICollection<T> GetAllPaging1(int page, int pageSize, out int totalRow, out int totalPage, Expression<Func<T, bool>> whereCondition = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string[] includes = null);
        int Count(Expression<Func<T, bool>> whereCondition);
        bool Exists(Expression<Func<T, bool>> whereCondition);
        ICollection<T> ExecWithStoreProcedure(string query, params object[] parameters);
        T FirstOrDefault(Expression<Func<T, bool>> whereCondition, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string[] includes = null);
        


    }
}
