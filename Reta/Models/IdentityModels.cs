using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Reta.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

    public class ApplicationDbInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            if (!(context.Users.AnyAsync(u => u.UserName == "vincent.lemaitre.01@gmail.com").Result))
            {
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);
                var userToInsert = new ApplicationUser { UserName = "vincent.lemaitre.01@gmail.com", Email= "vincent.lemaitre.01@gmail.com" , PhoneNumber = "0797697898" };
                userManager.Create(userToInsert, "pti.preu");

                // ADD ROLE SUPERVISOR TO USER
                var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                if (!RoleManager.RoleExists("Supervisor"))
                {
                    IdentityRole newRole = new IdentityRole("Supervisor");
                    IdentityResult result = RoleManager.CreateAsync(newRole).Result;                                 
                }

                userManager.AddToRoleAsync(userToInsert.Id, "Supervisor");
                
            }
            base.Seed(context);
        }
    }
        
}