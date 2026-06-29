using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Data.Context;
using Data.Extensions;
using Domain.Interfaces.Repositories;
using Domain.Entities;

namespace Data.Repositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        protected readonly SalaoCabeleleilaDbContext _context;
        private DbSet<TEntity> Entity => _context.Set<TEntity>();

        public BaseRepository(SalaoCabeleleilaDbContext contexto)
        {
            _context = contexto;
        }

        public TEntity Add(TEntity obj)
        {
            Entity.Add(obj);
            return obj;
        }

        public TEntity Update(TEntity obj)
        {
            _context.Entry(obj).State = EntityState.Modified;
            return obj;
        }

        public int Count()
        {
            return Entity.AsNoTracking().Count();
        }

        public int Count(Expression<Func<TEntity, bool>> where)
        {
            return Entity.AsNoTracking().Count(where);
        }

        public void Delete(TEntity obj)
        {
            _context.Entry(obj).State = EntityState.Deleted;
        }

        public void DeleteAll(IEnumerable<TEntity> objs)
        {
            foreach (var entity in objs)
                Delete(entity);
        }

        public bool Any(Expression<Func<TEntity, bool>> where)
        {
            return Entity.AsNoTracking().Any(where);
        }

        public async Task<TEntity> GetById(int Id, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            query = query.EagerLoad(includes);
            return await query.FirstOrDefaultAsync(f => (f as BaseEntity).Id.Equals(Id));
        }

        public async Task<IEnumerable<TEntity>> CustomFind(Expression<Func<TEntity, bool>> where)
        {
            return await _context.Set<TEntity>().Where(where).ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> CustomFind(
            Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            query = query.EagerLoad(includes);
            return await query.Where(where).ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> CustomFind(
            Expression<Func<TEntity, bool>> where,
            int start,
            int limit,
            params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            query = query.EagerLoad(includes);
            return await query.Where(where).Skip(start).Take(limit).ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> CustomFind(
            Expression<Func<TEntity, bool>> where,
            Expression<Func<TEntity, int>> orderby,
            params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            query = query.EagerLoad(includes);
            return await query.Where(where).OrderBy(orderby).ToListAsync();
        }

        public async Task<bool> Existe(Expression<Func<TEntity, bool>> where)
        {
            return await _context.Set<TEntity>().Where(where).AnyAsync();
        }

        public void Save(TEntity entity) => _context.Add(entity);

        public void SaveMany(IEnumerable<TEntity> entity) => _context.AddRange(entity);

        public void UpdateRange(List<TEntity> e) => _context.Set<TEntity>().UpdateRange(e);

        public async Task<IList<TEntity>> GetAll() => await _context.Set<TEntity>().ToListAsync();

        public async Task<ICollection<TEntity>> GetAllWithInclude(params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            query = query.EagerLoad(includes);
            return await query.ToListAsync();
        }

        public IQueryable<TEntity> Query() => _context.Set<TEntity>().AsQueryable();
    }
}
