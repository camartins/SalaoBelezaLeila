using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Domain.Interfaces.Repositories
{
    public interface IBaseRepository<TEntity>
    {
        void Save(TEntity entity);
        void SaveMany(IEnumerable<TEntity> entity);
        TEntity Update(TEntity obj);
        void UpdateRange(List<TEntity> e);
        void Delete(TEntity obj);
        void DeleteAll(IEnumerable<TEntity> obj);
        int Count();
        int Count(Expression<Func<TEntity, bool>> where);

        bool Any(Expression<Func<TEntity, bool>> where);

        Task<bool> Existe(Expression<Func<TEntity, bool>> where);

        Task<TEntity> GetById(int Id, params Expression<Func<TEntity, object>>[] includes);
        Task<IEnumerable<TEntity>> CustomFind(Expression<Func<TEntity, bool>> where);

        Task<IEnumerable<TEntity>> CustomFind(Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includes);

        Task<IEnumerable<TEntity>> CustomFind(Expression<Func<TEntity, bool>> where,
            int start, int limit, params Expression<Func<TEntity, object>>[] includes);

        Task<IEnumerable<TEntity>> CustomFind(Expression<Func<TEntity, bool>> where,
            Expression<Func<TEntity, int>> orderby,
            params Expression<Func<TEntity, object>>[] includes);

        Task<IList<TEntity>> GetAll();
        Task<ICollection<TEntity>> GetAllWithInclude(params Expression<Func<TEntity, object>>[] includes);
        IQueryable<TEntity> Query();
    }
}
