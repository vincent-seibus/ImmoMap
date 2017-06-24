using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.IO;
using System.Linq;
using System.Web;

namespace Reta.Models
{
    public class MySqlIdentityDbContext : DbContext
    {

        public MySqlIdentityDbContext()
            : base("DefaultConnection")
        {
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["InitializeDb"] != null ? ConfigurationManager.AppSettings["InitializeDb"] : "false"))
            {
               Database.SetInitializer<MySqlIdentityDbContext>(new MySqlDbInitializer());
            }
            else
            {
                Database.SetInitializer<MySqlIdentityDbContext>(null);
            }
        }
               
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<Space> Spaces { get; set; }
        public DbSet<Building> Buildings { get; set; }

    }
       

    public class MySqlDbInitializer : IDatabaseInitializer<MySqlIdentityDbContext>
    {
        public void InitializeDatabase(MySqlIdentityDbContext context)
        {
            if (!context.Database.Exists())
            {
                context.Database.Create();
            }    
        }

        protected void Seed(MySqlIdentityDbContext context)
        {
           
        }
    }

}