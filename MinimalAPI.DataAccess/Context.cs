using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MinimalAPI.DataAccess.Configurations;
using MinimalAPI.DataAccess.Models;

namespace MinimalAPI.DataAccess
{
    public class Context : DbContext
    {
        private const string TABLE_PREFIX = "MinimalApi";
        private readonly DatabaseConnectionManager _databaseConnectionManager;
        
        public DbSet<Customer> Customers { get; set; }
        
        public Context()
        {
            _databaseConnectionManager = new DatabaseConnectionManager();
        }

        public Context(IOptions<DatabaseConnectionManager> databaseConnectionManagerOption)
        {
            _databaseConnectionManager = databaseConnectionManagerOption.Value;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql($"host={_databaseConnectionManager.Host};" +
                                     $"port={_databaseConnectionManager.Port};" +
                                     $"database={_databaseConnectionManager.Database};" +
                                     $"user id={_databaseConnectionManager.Username};" +
                                     $"password={_databaseConnectionManager.Password}",
                x => x.MigrationsHistoryTable($"{TABLE_PREFIX}_MigrationHistory"));
            
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CustomerConfiguration());
            
            foreach (var entity in modelBuilder.Model.GetEntityTypes().Where(e => !e.IsOwned()))
            {
                entity.SetTableName($"{TABLE_PREFIX}_{entity.ClrType.Name}");
            }

        }
    }
}