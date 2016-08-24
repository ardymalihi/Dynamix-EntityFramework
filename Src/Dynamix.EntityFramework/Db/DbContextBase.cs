using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Data.Entity.Migrations;

namespace Dynamix.EntityFramework.Db
{
    public class DbContextBase : DbContext
    {
        public sealed class MigrationConfiguration : DbMigrationsConfiguration<DbContextBase> 
        { 
            public MigrationConfiguration() 
            { 
                this.AutomaticMigrationsEnabled = false; 
            } 
        }

        public DbContextBase()
        {
        }

        public DbContextBase(string ConnectionString)
        {
            this.Database.Connection.ConnectionString = ConnectionString;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        public void CancelChanges()
        {
            foreach (DbEntityEntry entry in this.ChangeTracker.Entries())
            {
                if (entry.CurrentValues != entry.OriginalValues)
                {
                    entry.Reload();
                }
            }
        }

        public void CancelChanges<T>() where T : class
        {
            foreach (DbEntityEntry<T> entry in this.ChangeTracker.Entries<T>())
            {
                if (entry.CurrentValues != entry.OriginalValues)
                {
                    entry.Reload();
                }
            }
        }

        public override int SaveChanges()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    sb.AppendLine(string.Format("Type \"{0}\" in state \"{1}\" has these validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        sb.AppendLine(string.Format("Property Name: \"{0}\", Error Message: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage));
                    }
                }

                throw new System.Exception(sb.ToString(), e);
            }
            catch (System.Exception exp)
            {
                throw new System.Exception("Error ContextBase.SaveChanges Method", exp);
            }
        }
    }
}
