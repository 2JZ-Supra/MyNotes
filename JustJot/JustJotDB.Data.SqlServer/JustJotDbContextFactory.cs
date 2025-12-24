using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace JustJotDB.Data.SqlServer
{
    public class JustJotDbContextFactory : IDesignTimeDbContextFactory<JustJotDbContext>
    {
        public JustJotDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.database.json")
                .Build();
            return CreateDbContext(configuration);
        }

        public JustJotDbContext CreateDbContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var optionsBuilder = new DbContextOptionsBuilder<JustJotDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return new JustJotDbContext(optionsBuilder.Options);
        }
    }
}