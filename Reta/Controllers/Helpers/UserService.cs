using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using Newtonsoft.Json.Linq;
using Reta.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;

namespace Reta.Controllers.Helpers
{
    public static class UserService
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(UserService));
        
        internal static bool UserExist(string userId)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);

            var user = UserManager.FindById(userId);

            if (user == null)
                user = UserManager.FindByName(userId);

            if (user == null)
                return false;

            return true;

        }

        public static ApplicationUser getUserByEmail(string email)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);
            if (!string.IsNullOrEmpty(email))
            {
                var user = UserManager.FindByEmail(email);
                if (user != null)
                    return user;

                user = UserManager.FindByName(email);
                if (user != null)
                    return user;
            }
               
            return null;
        }

        public static ApplicationUser getUserById(string id)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);
            if (!string.IsNullOrEmpty(id))
            {
                var user = UserManager.FindById(id);
                return user;             
            }
            return null;
        }

        public static bool createUser(ApplicationUser user)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);
            var result = UserManager.Create(user);
            return result.Succeeded;
        }

        public static bool deleteUser(ApplicationUser user)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);
            return UserManager.Delete(UserManager.FindById(user.Id)).Succeeded;
        }

        /// <summary>
        /// Add the  privileges of the role specify in the file App_Data/roles.json 
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="role">if null the role added is the role in the configuration file</param>
        public static void addUserInRole(ApplicationUser user, string role = null)
        {
            try
            {
                if(string.IsNullOrEmpty(role))
                    role = ConfigurationManager.AppSettings["AutoRegistration:UserRoleDefault:Name"];

                if (!string.IsNullOrEmpty(role))
                {
                    ApplicationDbContext context = new ApplicationDbContext();
                    IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
                    ApplicationUserManager UserManager = new ApplicationUserManager(store);

                    string roleJson = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/App_Data/roles.json"));
                    JObject roles = JObject.Parse(roleJson);
                    JArray arrayOfRoles = (JArray)roles["roles"];
                    foreach (JObject r in arrayOfRoles)
                    {
                        if (r["name"].ToString() == role)
                        {
                            foreach (var privilege in r["privileges"])
                            {
                                addUserInPrivilege(user, privilege.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("the user : " + user.UserName + " has not been associated to the default role - error :" + ex.Message);
            }
        }
        /// <summary>
        /// Add the  privilege to the user if  not yet, create privilege if not exist
        /// </summary>
        /// <param name="user"></param>
        /// <param name="privilege"></param>
        public static void addUserInPrivilege(ApplicationUser user, string privilege)
        {
            try {
                ApplicationDbContext context = new ApplicationDbContext();
                IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
                ApplicationUserManager UserManager = new ApplicationUserManager(store);

                if (!UserManager.IsInRole(user.Id, privilege))
                {

                    var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

                    if (!RoleManager.RoleExists(privilege))
                    {
                        IdentityRole newRole = new IdentityRole(privilege);
                        IdentityResult result = RoleManager.CreateAsync(newRole).Result;
                        if (result.Succeeded)
                            UserManager.AddToRole(user.Id, privilege);
                    }
                    else
                    {
                        UserManager.AddToRole(user.Id, privilege);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("the user : " + user.UserName + " has not been associated to the privilege : " + privilege + " - error :" + ex.Message);
            }
        }

        /// <summary>
        /// retrieve the language of the user passed in parameter which is stored in the claims
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static Language getUserLanguage(ApplicationUser user)
        {            
            var claimLanguage = user.Claims.Where(a => a.ClaimType == "Language").FirstOrDefault();
            if (claimLanguage == null)
                return Language.fr;

            var langValue = claimLanguage.ClaimValue;
            Language lang = (Language)Enum.Parse(typeof(Language), langValue);
            return lang;
        }

        /// <summary>
        /// generate a code for a user to be able to login automatically without having to put password
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string generateCodeForAutoLogin(ApplicationUser user)
        {
            string token = generateTokenForAutoLogin(user);
            string salt = "4dfb2bf9d4bd48e3851f834cf1cd78401e71e43e968e45ccd8b7cc32cae9d4122ffed766bfea046b97a6f8064ba697137126b5f6a3957a2b4b8e8bb9b62d7ace";
            byte[] emailBytes = Encoding.ASCII.GetBytes(user.Email);
            string emailEncoded = Convert.ToBase64String(emailBytes);
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(token + salt + emailEncoded)); 
        }

        /// <summary>
        /// generate a token for a user to be able to login automatically without having to put password
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string generateTokenForAutoLogin(ApplicationUser user)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);
            var token = sha256(user.Id);
            return token;
        }

        /// <summary>
        /// check token that 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool verifyCodeForAutoLogin(string code)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);
            byte[] codeBytes = Convert.FromBase64String(code);
            string codeString = Encoding.ASCII.GetString(codeBytes);
            string token = "";
            string salt = "4dfb2bf9d4bd48e3851f834cf1cd78401e71e43e968e45ccd8b7cc32cae9d4122ffed766bfea046b97a6f8064ba697137126b5f6a3957a2b4b8e8bb9b62d7ace";
            ApplicationUser user;
            var codeArray = codeString.Split(new[] { salt }, StringSplitOptions.None);
            if (codeArray.Length != 2)
                return false;
            token = codeArray[0];
            user = UserManager.FindByEmail(Encoding.ASCII.GetString(Convert.FromBase64String(codeArray[1])));
            if(user == null)
                return false;
            if (sha256(user.Id) == token)
                return true;

            return false;
        }

        public static ApplicationUser getUserFromCode(string code)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);
            byte[] codeBytes = Convert.FromBase64String(code);
            string codeString = Encoding.ASCII.GetString(codeBytes);
            string token = "";
            string salt = "4dfb2bf9d4bd48e3851f834cf1cd78401e71e43e968e45ccd8b7cc32cae9d4122ffed766bfea046b97a6f8064ba697137126b5f6a3957a2b4b8e8bb9b62d7ace";
            ApplicationUser user;
            var codeArray = codeString.Split(new[] { salt }, StringSplitOptions.None);
            if (codeArray.Length != 2)
                return null;
            token = codeArray[0];
            user = UserManager.FindByEmail(Encoding.ASCII.GetString(Convert.FromBase64String(codeArray[1])));
            return user;
        }

        public static string sha256(string id)
        {
            SHA256Managed crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(id), 0, Encoding.ASCII.GetByteCount(id));
            foreach (byte theByte in crypto)
            {
                hash += theByte.ToString("x2");
            }
            return hash;
        }
    }

    public enum Language
    {
        fr,
        en,
    }
}