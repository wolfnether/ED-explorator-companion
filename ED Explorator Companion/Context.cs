using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace ED_Explorator_Companion
{
    internal class Context : DbContext
    {
        public DbSet<StarSystem> StarSystems { get; set; }
        public DbSet<Body> Bodies { get; set; }
        public DbSet<ConfigPair> ConfigPair { get; set; }

        public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseLoggerFactory(MyLoggerFactory).UseSqlite("Data Source=db.db");
            options.EnableSensitiveDataLogging();
        }
    }
}