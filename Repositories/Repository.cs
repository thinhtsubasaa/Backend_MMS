using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ERP.Data;
using ERP.Infrastructure;

namespace ERP.Repositories
{
    public abstract class Repository<T> : IRepository<T> where T : class
    {
        protected readonly MyDbContext _db;
        private readonly DbSet<T> _dbSet;
        protected Repository(MyDbContext db)
        {
            _db = db;
            _dbSet = db.Set<T>();
        }
        public int Count(Expression<Func<T, bool>> whereCondition)
        {
            return _dbSet.Count(whereCondition);
        }
        public bool Exists(Expression<Func<T, bool>> whereCondition)
        {
            return _db.Set<T>().Count<T>(whereCondition) > 0;
        }
        public T GetSingle(Expression<Func<T, bool>> whereCondition, string[] includes = null)
        {
            T _resetSet;
            //HANDLE INCLUDES FOR ASSOCIATED OBJECTS IF APPLICABLE
            if (includes != null && includes.Count() > 0)
            {
                var query = _db.Set<T>().Include(includes.First());
                foreach (var include in includes.Skip(1))
                    query = query.Include(include);
                _resetSet = query.Where<T>(whereCondition).FirstOrDefault();
            }
            else
            {
                _resetSet = _db.Set<T>().Where<T>(whereCondition).FirstOrDefault();
            }
            return _resetSet;
        }
        public ICollection<T> GetAll_No_Condition(Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string[] includes = null)
        {
            ICollection<T> _resetSet;
            //HANDLE INCLUDES FOR ASSOCIATED OBJECTS IF APPLICABLE
            if (includes != null && includes.Count() > 0)
            {
                var query = _db.Set<T>().Include(includes.First());
                foreach (var include in includes.Skip(1))
                    query = query.Include(include);
                _resetSet = query.ToList();
                if (orderBy != null)
                {
                    _resetSet = orderBy(_resetSet.AsQueryable<T>()).ToList();
                }
            }
            else
            {
                _resetSet = _db.Set<T>().ToList();
                if (orderBy != null)
                {
                    _resetSet = orderBy(_resetSet.AsQueryable<T>()).ToList();
                }
            }
            return _resetSet.ToList<T>();
        }
        public ICollection<T> GetAll(Expression<Func<T, bool>> whereCondition, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string[] includes = null)
        {
            ICollection<T> _resetSet;
            //HANDLE INCLUDES FOR ASSOCIATED OBJECTS IF APPLICABLE
            if (includes != null && includes.Count() > 0)
            {
                var query = _db.Set<T>().Include(includes.First());
                foreach (var include in includes.Skip(1))
                    query = query.Include(include);
                _resetSet = query.Where<T>(whereCondition).ToList();
                if (orderBy != null)
                {
                    _resetSet = orderBy(_resetSet.AsQueryable<T>()).ToList();
                }
            }
            else
            {
                _resetSet = _db.Set<T>().Where<T>(whereCondition).ToList();
                if (orderBy != null)
                {
                    _resetSet = orderBy(_resetSet.AsQueryable<T>()).ToList();
                }
            }
            return _resetSet.ToList<T>();
        }
        public ICollection<T> GetAllPaging(Expression<Func<T, bool>> whereCondition, out int total, int page, int pageSize, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string[] includes = null)
        {
            ICollection<T> _resetSet;
            //HANDLE INCLUDES FOR ASSOCIATED OBJECTS IF APPLICABLE
            if (includes != null && includes.Count() > 0)
            {
                var query = _db.Set<T>().Include(includes.First());
                foreach (var include in includes.Skip(1))
                    query = query.Include(include);
                _resetSet = query.Where<T>(whereCondition).ToList();
                if (orderBy != null)
                {
                    _resetSet = orderBy(_resetSet.AsQueryable<T>()).ToList();
                }
            }
            else
            {
                _resetSet = _db.Set<T>().Where<T>(whereCondition).ToList();
                if (orderBy != null)
                {
                    _resetSet = orderBy(_resetSet.AsQueryable<T>()).ToList();
                }
            }
            total = _resetSet.Count();
            _resetSet = _resetSet.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return _resetSet.ToList<T>();
        }
        public ICollection<T> GetAllPaging1(int page, int pageSize, out int totalRow, out int totalPage, Expression<Func<T, bool>> whereCondition = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string[] includes = null)
        {
            IQueryable<T> query = _dbSet;
            if (whereCondition != null)
            {
                query = query.Where(whereCondition);
            }
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            totalRow = query.Count();
            totalPage = (int)Math.Ceiling(totalRow / (double)pageSize);
            query = query.Skip((page - 1) * pageSize).Take(pageSize);
            if (includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return query.AsNoTracking().ToList();
        }
        void IRepository<T>.Delete(object id)
        {
            var toDelete = _dbSet.Find(id);
            Delete(toDelete);
        }
        public virtual void Delete(T entity)
        {
            _dbSet.Attach(entity);
            _dbSet.Remove(entity);
        }
        void IRepository<T>.DeleteRange(List<object> lst)
        {
            foreach (var id in lst)
            {
                var toDelete = _dbSet.Find(id);
                Delete(toDelete);
            }
        }
        T IRepository<T>.GetById(object id)
        {
            return _dbSet.Find(id);
        }
        T IRepository<T>.Add(T entity)
        {
            return _dbSet.Add(entity).Entity;
        }
        void IRepository<T>.Update(T entity)
        {
            _dbSet.Attach(entity);
            _db.Entry(entity).State = EntityState.Modified;
        }
        ICollection<T> IRepository<T>.ExecWithStoreProcedure(string query, params object[] parameters)
        {
            return _dbSet.FromSqlRaw<T>(query, parameters).ToList<T>();
        }
        void IRepository<T>.AddRange(IEnumerable<T> entities)
        {
            _dbSet.AddRange(entities);
        }
        void IRepository<T>.UpdateRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                _dbSet.Attach(entity);
                _db.Entry(entity).State = EntityState.Modified;
            }
        }
        void IRepository<T>.RemoveRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                _dbSet.Attach(entity);
                _dbSet.Remove(entity);
            }
        }

        public T FirstOrDefault(Expression<Func<T, bool>> whereCondition, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string[] includes = null)
        {
            IQueryable<T> query = _db.Set<T>();
            if (includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            query = query.Where(whereCondition);

            if (orderBy != null)
            {
                query = orderBy(query);
            }
            return query.FirstOrDefault();
        }

    }
}