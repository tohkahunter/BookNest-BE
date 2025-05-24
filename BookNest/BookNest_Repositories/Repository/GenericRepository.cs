using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BookNest_Repositories.Interface;
using BookNest_Repositories.Models;

namespace BookNest_Repositories.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly BookTracker7Context _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(BookTracker7Context context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.Where(expression).ToListAsync();
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.FirstOrDefaultAsync(expression);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
            _context.SaveChanges();
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            _context.SaveChanges();
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.AnyAsync(expression);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> expression = null)
        {
            if (expression == null)
                return await _dbSet.CountAsync();
            return await _dbSet.CountAsync(expression);
        }

        public async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<T>> IncludeAsync(Expression<Func<T, object>> include)
        {
            return await _dbSet.Include(include).ToListAsync();
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
            _context.SaveChanges();
        }
    }
}
