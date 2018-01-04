using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using WorkAdmin.Models.Entities;
using System.Configuration;

namespace WorkAdmin.Logic
{
    public class MyDbContext : DbContext, IDisposable
    {
        public MyDbContext()
            : base(nameOrConnectionString: "DefaultConnection")
        {
            string autoMigrationSetting = ConfigurationManager.AppSettings["AutoDbMigration"];
            bool autoMigration = false;
            bool.TryParse(autoMigrationSetting, out autoMigration);
            if (!autoMigration)
                Database.SetInitializer<MyDbContext>(null);
            else
                Database.SetInitializer<MyDbContext>(new MigrateDatabaseToLatestVersion<MyDbContext, MyAppConfiguation>());
        }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<WorkLog> WorkLogs { get; set; }

        public virtual DbSet<WorkReport> WorkReports { get; set; }

        public virtual DbSet<WorkReportProperty> WorkReportProperties { get; set; }

        public virtual DbSet<WorklogTime> Record { get; set; }

        public virtual DbSet<WorkLogProperty> WorkLogProperties { get; set; }

        public virtual DbSet<UserPosition> UserPositions { get; set; }

        public virtual DbSet<WorkReportRecord> WorkReportRecords { get; set; }
    }

    public class MyAppConfiguation : System.Data.Entity.Migrations.DbMigrationsConfiguration<MyDbContext>
    {
        public MyAppConfiguation()
        {
            this.AutomaticMigrationsEnabled = true;
            this.AutomaticMigrationDataLossAllowed = true;
        }
    }
}
