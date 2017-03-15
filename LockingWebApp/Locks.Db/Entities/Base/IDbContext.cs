using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LockingWebApp.Locks.Db.Entities.Base
{
    public interface IDbContext
    {
        void Attach<T>(T entity) where T : class, IGuidEntity;
        IQueryable<T> AsQueryable<T>() where T : class, IGuidEntity;
        void Update<T>(T entity) where T : class, IGuidEntity;
        void Save<T>(T entity) where T : class, IGuidEntity;

        int SaveChanges();
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        DbEntityEntry Entry(object entity);
        DbSet Set(Type entityType);
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
}