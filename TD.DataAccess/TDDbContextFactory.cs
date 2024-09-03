using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TD.DataAccess
{
    public class TDDbContextFactory : IDesignTimeDbContextFactory<TDDbContext>
    {
        public TDDbContextFactory()
        {
        }

        public TDDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<TDDbContext>();
            return new TDDbContext(builder.Options);
        }
    }
}
