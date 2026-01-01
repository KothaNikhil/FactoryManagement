using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.IO;

namespace FactoryManagement.Data
{
    /// <summary>
    /// Design-time factory for creating DbContext during migrations
    /// </summary>
    public class FactoryDbContextFactory : IDesignTimeDbContextFactory<FactoryDbContext>
    {
        public FactoryDbContext CreateDbContext(string[] args)
        {
            var appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Factory Management");
            Directory.CreateDirectory(appDataDir);
            var dbPath = Path.Combine(appDataDir, "factory.db");

            var optionsBuilder = new DbContextOptionsBuilder<FactoryDbContext>();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            return new FactoryDbContext(optionsBuilder.Options);
        }
    }
}
