using Microsoft.EntityFrameworkCore;
using TD.Domain.Entities;
using Serilog;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace TD.DataAccess
{
    public class TDDbContext : DbContext
    {
        public static Func<string> LoggerFunction;

        public TDDbContext(DbContextOptions<TDDbContext> options) : base(options)
        {
            //Log.Information(this.Database.GetDbConnection().ConnectionString);
            try
            {
                if (LoggerFunction != null)
                {
                    Log.Information(LoggerFunction());
                }
            }
            catch (Exception ex) { Log.Error(ex, "{exception}",ex); }
            Thread thread = Thread.CurrentThread;
            var msg = String.Format(
               String.Format("   Background: {0}\n", thread.IsBackground) +
               String.Format("   Thread Pool: {0}\n", thread.IsThreadPoolThread) +
               String.Format("   Thread ID: {0}\n", thread.ManagedThreadId) +
               String.Format("   Instance ID: {0}\n", this.ContextId));
            //Log.Information(msg);
        }
        public DbSet<RegisteredUser> RegisteredUsers { get; set; }
        public DbSet<Prefix> Prefixes { get; set; }

        public DbSet<RoleName> RoleNames { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);


        }

    }
    public static class DbSetExtensions
    {
        public static T AddIfNotExists<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>> predicate = null) where T : class, new()
        {

            var exists = predicate != null ? dbSet.Any(predicate) : dbSet.Any();
            return exists ? dbSet.Add(entity).Entity : null;
        }
        //public static T AddOrUpdate<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>> predicate = null) where T : class, new()
        //{
        //    var exists = predicate != null ? dbSet.Any(predicate) : dbSet.Any();
        //    return exists ? dbSet.Add(entity).Entity : dbSet.Update(entity).Entity;
        //}
    }
}
