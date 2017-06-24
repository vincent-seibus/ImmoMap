using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Reta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reta.Controllers.Helpers
{
    public class SpaceService
    {
        public SpaceService()
        {

        }

        public SpaceService(string UserId)
        {
            GetSpaceIdFromUser(UserId);
        }

        public Space Space { get; set; }
        public string GetSpaceIdFromUser(string UserId)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            MySqlIdentityDbContext db = new MySqlIdentityDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);
            var claims = UserManager.GetClaims(UserId);
            var claim = claims.Where(a => a.Type == "SpaceId").FirstOrDefault();

            if (claim != null)
            {
                Space = db.Spaces.Find(claim.Value);
                if(Space != null)
                    return Space.Id;
            }

            return null;
        }
    }
}