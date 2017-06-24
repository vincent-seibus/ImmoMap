using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Reta.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));

        // GET: Accounts
        [Authorize(Roles = "Supervisor,ViewUsersList")]
        public string GetUsers()
        {
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                //IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
                //ApplicationUserManager UserManager = new ApplicationUserManager(store);
                var users = context.Users;
                return JsonConvert.SerializeObject(users.ToList());
            }
            catch(Exception ex)
            {
                logger.Error(" User :" + User.Identity.Name + " - message : " + ex.Message);
                return JsonConvert.SerializeObject("");
            }
            
        }

        // POST: Accounts
        [Authorize(Roles = "Supervisor,ModifyUsers")]
        [HttpPost]
        public string PostCreateUser(string value)
        {
            try
            {
                JObject userJson = JObject.Parse(value);
                ApplicationDbContext context = new ApplicationDbContext();
                IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
                ApplicationUserManager UserManager = new ApplicationUserManager(store);

                ApplicationUser user = new ApplicationUser();
                user.Email = userJson["Email"].ToString();
                user.UserName = userJson["Email"].ToString();
                string password = userJson["Password"].ToString();
                string spaceId = userJson["SpaceId"].ToString();                
                var result = UserManager.Create(user, password);

                if (result.Succeeded)
                    UserManager.AddClaimAsync(UserManager.FindByEmail(user.Email).Id, new Claim("SpaceId", spaceId));

                // log creation user
                logger.Info(" User :" + User.Identity.Name + " - PostCreateUser : " + value);
                return JsonConvert.SerializeObject(new { error = false });
            }
            catch(Exception ex)
            {
                logger.Error(" User :" + User.Identity.Name + " - message : " + ex.Message);
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "UserCreationFailed"});
            }
        }

        [Authorize(Roles = "Supervisor,ModifyUsers")]
        [HttpPost]
        public string PostEditUser(string value)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<ApplicationUser>(value);
                ApplicationDbContext context = new ApplicationDbContext();
                IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
                ApplicationUserManager UserManager = new ApplicationUserManager(store);
                var userFound = UserManager.FindById(user.Id);
                userFound.AccessFailedCount = user.AccessFailedCount;
                userFound.Email = user.Email;
                userFound.EmailConfirmed = user.EmailConfirmed;
                userFound.LockoutEnabled = user.LockoutEnabled;
                userFound.LockoutEndDateUtc = user.LockoutEndDateUtc;
                userFound.PhoneNumber = user.PhoneNumber;
                userFound.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                userFound.TwoFactorEnabled = user.TwoFactorEnabled;
                userFound.UserName = user.UserName;
                UserManager.Update(userFound);

                // log creation user
                logger.Info(" User :" + User.Identity.Name + " - PostCreateUser : " + value);

                return JsonConvert.SerializeObject(new { error = false });
            }
            catch (Exception ex)
            {
                logger.Error(" User :" + User.Identity.Name + " - message : " + ex.Message);
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "EditUserFailed" });
            }
        }

        [Authorize(Roles = "Supervisor,ModifyUsers")]
        [HttpPost]
        public string PostDeleteUser(string value)
        {
            var user = JsonConvert.DeserializeObject<ApplicationUser>(value);
            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);

            var userFound = UserManager.FindById(user.Id);

            if (userFound != null)
                UserManager.Delete(userFound);
            else
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "UserNotExist" });


            // log creation user
            logger.Info(" User :" + User.Identity.Name + " - PostDeleteUser : " + value);

            return JsonConvert.SerializeObject(new { error = false });
           
        }

        // GET: Accounts
        // [Authorize(Roles = "Supervisor,ModifyUserRoles")]
        public string GetPrivileges()
        { 
            string privilegesJson = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/App_Data/privileges.json"));           
            return privilegesJson;
        }

        public string GetRoles(string value)
        {
            JObject val = JObject.Parse(value);
            var userId = val["userId"].ToString();
            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);
            var roles = UserManager.GetRoles(userId);
            return JsonConvert.SerializeObject(roles);      
        }

        [Authorize(Roles = "Supervisor,ModifyUserRoles")]
        [HttpPost]
        public async Task<string> PostEditUserRoles(string value)
        {
            JObject val = JObject.Parse(value);
            var privilege = val["name"].ToString();
            var id = val["id"].ToString();

            ApplicationDbContext context = new ApplicationDbContext();           
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);

            if (!UserManager.IsInRole(id, privilege))
            {

                var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

                if (!RoleManager.RoleExists(privilege))
                {
                    IdentityRole newRole = new IdentityRole(privilege);
                    IdentityResult result = await RoleManager.CreateAsync(newRole);
                    if (result.Succeeded)
                        await UserManager.AddToRoleAsync(id, privilege);
                    else
                        return JsonConvert.SerializeObject(new { error = true });
                }
                else
                {
                    await UserManager.AddToRoleAsync(id, privilege);
                }
                                
            }

            // log 
            logger.Info(" User :" + User.Identity.Name + " - PostEditUserRoles : " + value);

            var roles = UserManager.GetRoles(id);      
            return JsonConvert.SerializeObject(new { error = false, response = roles});

        }

        [Authorize(Roles = "Supervisor,ModifyUserRoles")]
        [HttpPost]
        public async Task<string> PostDeleteUserRoles(string value)
        {
            JObject val = JObject.Parse(value);
            var privilege = val["name"].ToString();
            var id = val["id"].ToString();

            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (UserManager.IsInRole(id, privilege))
            {
                await UserManager.RemoveFromRolesAsync(id, privilege);
            }

            // log 
            logger.Info(" User :" + User.Identity.Name + " - PostDeleteUserRoles : " + value);

            var roles = UserManager.GetRoles(id);
            return JsonConvert.SerializeObject(new { error = false, response = roles });
        }

        [Authorize(Roles = "Supervisor,LogAsUser")]
        [HttpPost]
        public async Task<string> PostLogAs(string value)
        {
            JObject val = JObject.Parse(value);
            var id = val["id"].ToString();

            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);
            ApplicationSignInManager SignInManager = new ApplicationSignInManager(UserManager, HttpContext.GetOwinContext().Authentication);

            var logAsUser = UserManager.FindById(id);
            logger.Info(" User :" + User.Identity.Name + " - PostLogAs : " + logAsUser.UserName);
            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            await SignInManager.SignInAsync(logAsUser, false, false);  
                      
            return JsonConvert.SerializeObject(new { error = false });
        }

        // POST: Accounts
        [Authorize(Roles = "Supervisor,ModifyUsers")]
        [HttpPost]
        public string PostEditUserSpace(string value)
        {
            try
            {
                JObject userSpace = JObject.Parse(value);
                ApplicationDbContext context = new ApplicationDbContext();
                IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
                ApplicationUserManager UserManager = new ApplicationUserManager(store);
                
                var userid = userSpace["userId"].ToString();
                var spaceId = userSpace["spaceId"].ToString();

                var claims = UserManager.GetClaims(User.Identity.GetUserId());
                var claim = claims.Where(a => a.Type == "SpaceId").FirstOrDefault();

                if (claim == null)
                {
                    UserManager.AddClaim(userid, new Claim("SpaceId", spaceId));
                }
                else
                {
                    UserManager.RemoveClaim(userid, claim);
                    UserManager.AddClaim(userid, new Claim("SpaceId", spaceId));
                }

                // log creation user
                logger.Info(" User :" + User.Identity.Name + " - PostEditUserSpace : " + value);
                return JsonConvert.SerializeObject(new { error = false });
            }
            catch (Exception ex)
            {
                logger.Error(" User :" + User.Identity.Name + " - message : " + ex.Message);
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "PostEditUserSpaceFailed" });
            }
        }

        #region helpers

        #endregion
    }
}