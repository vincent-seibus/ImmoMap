using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Reta.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using System.Security.Claims;
using log4net;

namespace Reta.Controllers
{
    [Authorize]
    public class SmtpConfigurationUsersController : Controller
    {
        private MySqlIdentityDbContext db = new MySqlIdentityDbContext();
        private static readonly ILog logger = LogManager.GetLogger(typeof(SmtpConfigurationUsersController));

        private ApplicationUserManager _userManager;

        public SmtpConfigurationUsersController()
        {
        }

        public SmtpConfigurationUsersController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        // GET: SmtpConfigurationUsers/Edit/5
        public ActionResult Edit()
        {

            ViewBag.Success = "none";
            ViewBag.Error = "none";

            if (User == null || !User.Identity.IsAuthenticated)
            {               
                return View();
            }

            var claims = UserManager.GetClaims(User.Identity.GetUserId());
            var smtpConfigJson = claims.Where(a => a.Type == "SmtpConfiguration").FirstOrDefault();

            if (smtpConfigJson == null)
            {             
                return View();              
            }

            try
            {
                var smtpConfig = JsonConvert.DeserializeObject<SmtpConfigurationUser>(smtpConfigJson.Value);
               
                return View(smtpConfig);                
            }
            catch(Exception ex)
            {                
                return View();
            }

        }

        // POST: SmtpConfigurationUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "host,port,username,password,enableSsl,timeout")] SmtpConfigurationUser smtpConfigurationUser)
        {
            if (User == null || !User.Identity.IsAuthenticated)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (ModelState.IsValid)
            {
                var claims = UserManager.GetClaims(User.Identity.GetUserId());

                var smtpConfigJson = claims.Where( a => a.Type == "SmtpConfiguration").FirstOrDefault();

                if (smtpConfigJson == null)
                {
                    UserManager.AddClaim(User.Identity.GetUserId(), new Claim("SmtpConfiguration", JsonConvert.SerializeObject(smtpConfigurationUser)));                                      
                }
                else
                {
                    UserManager.RemoveClaim(User.Identity.GetUserId(), smtpConfigJson);
                    UserManager.AddClaim(User.Identity.GetUserId(), new Claim("SmtpConfiguration", JsonConvert.SerializeObject(smtpConfigurationUser)));

                }
                ViewBag.Success = "block";
                ViewBag.Error = "none";
              
                return View(smtpConfigurationUser);
            }

            ViewBag.Success = "none";
            ViewBag.Error = "block";
            return View(smtpConfigurationUser);
        }

        [HttpPost]
        public string PostEditSmtp(string value)
        {
            if (User == null || !User.Identity.IsAuthenticated)
            {
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "NoUser" });
            }

            try { 
                var smtpConfigurationUser = JsonConvert.DeserializeObject<SmtpConfigurationUser>(value);
                var claims = UserManager.GetClaims(User.Identity.GetUserId());

                var smtpConfigJson = claims.Where(a => a.Type == "SmtpConfiguration").FirstOrDefault();

                if (smtpConfigJson == null)
                {
                    UserManager.AddClaim(User.Identity.GetUserId(), new Claim("SmtpConfiguration", JsonConvert.SerializeObject(smtpConfigurationUser)));
                }
                else
                {
                    UserManager.RemoveClaim(User.Identity.GetUserId(), smtpConfigJson);
                    UserManager.AddClaim(User.Identity.GetUserId(), new Claim("SmtpConfiguration", JsonConvert.SerializeObject(smtpConfigurationUser)));

                }
                var user = UserManager.FindById(User.Identity.GetUserId());
                return JsonConvert.SerializeObject(new { error = false, user = user });
            }
            catch(Exception ex)
            {
                logger.Error(ex.Message);
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "serverError" });
            }
            
        }
    }
}
