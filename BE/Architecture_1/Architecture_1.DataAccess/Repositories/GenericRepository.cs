using Architecture_1.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;

namespace Architecture_1.DataAccess.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext _dbContext;
        internal DbSet<T> _dbSet;

        public GenericRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public async Task<T?> FindByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> FindAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> CreateAsync(T entity)
        {
            var entry = await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entry.Entity; // Trả về entity vừa thêm (có thể chứa ID tự sinh)
        }

        public async Task<T?> UpdateAsync(int id, T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Entity không được để null.");
            }

            var existingEntity = await _dbSet.FindAsync(id);
            if (existingEntity == null)
            {
                return null;
            }

            _dbContext.Entry(existingEntity).State = EntityState.Detached;
            _dbContext.Entry(entity).State = EntityState.Modified;

            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<T?> DeleteAsync(object id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _dbContext.SaveChangesAsync();
                return entity;
            }

            return null;
        }


        //public async Task CreateAsync(T entity)
        //{
        //    await _dbSet.AddAsync(entity);
        //    await _dbContext.SaveChangesAsync();
        //}
        //public async Task UpdateAsync(int id, T entity)
        //{
        //    if (entity == null)
        //    {
        //        throw new ArgumentNullException(nameof(entity), "Entity không được để null.");
        //    }

        //    var existingEntity = await _dbSet.FindAsync(id);
        //    if (existingEntity == null)
        //    {
        //        throw new Exception($"Entity với Id = {id} không tồn tại.");
        //    }

        //    Console.WriteLine("Entity: " + entity.ToString());

        //    // Đảm bảo entity được tracking
        //    _dbContext.Entry(existingEntity).State = EntityState.Detached; // Ngắt tracking entity cũ
        //    _dbContext.Entry(entity).State = EntityState.Modified; // Mark entity là Modified

        //    await _dbContext.SaveChangesAsync();
        //}
        //public async Task DeleteAsync(object id)
        //{
        //    var entity = await _dbSet.FindAsync(id);
        //    if (entity != null)
        //    {
        //        _dbSet.Remove(entity);
        //        await _dbContext.SaveChangesAsync();
        //    }
        //}
    }
}
