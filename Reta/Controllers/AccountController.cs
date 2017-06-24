using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Reta.Models;
using log4net;
using Newtonsoft.Json;
using System.Configuration;
using Reta.Controllers.Helpers;
using System.Net.Mail;
using System.Web.Hosting;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.IO;
using System.Net.Mime;

namespace Reta.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountController));
        private MySqlIdentityDbContext db = new MySqlIdentityDbContext();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
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
        // GET: /Accoutn/isConnected
        [AllowAnonymous]
        public string IsConnected()
        {
            if (User.Identity.IsAuthenticated)
                return JsonConvert.SerializeObject(new { isConnected = true });
            else
                return JsonConvert.SerializeObject(new { isConnected = false });
        }

        // GET: /Accoutn/userInfo
        public string userInfo()
        {
           var user = UserManager.FindById(User.Identity.GetUserId());           
            var roles = UserManager.GetRoles(User.Identity.GetUserId());
            var ClaimSpaceId = UserManager.GetClaims(user.Id).Where(a => a.Type == "SpaceId").FirstOrDefault();

            Space space = null;
            if (ClaimSpaceId != null)
                space = db.Spaces.Find(ClaimSpaceId.Value);

            return JsonConvert.SerializeObject(new { error = false, user = user, roles = roles, space = space });                       
        }

        // GET : /Account/getUserRoles
        public async Task<string> getUserRoles()
        {           
            var roles = await UserManager.GetRolesAsync(User.Identity.GetUserId());
            return JsonConvert.SerializeObject(roles);

        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public string PostLogin(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                logger.Error("Model Invalid");
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "modelInvalid" });
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false).Result;
            switch (result)
            {
                case SignInStatus.Success:
                    return JsonConvert.SerializeObject(new { error = false });
                case SignInStatus.LockedOut:
                    return JsonConvert.SerializeObject(new { error = true, errorMessage = "accountLockedOut" });
                case SignInStatus.RequiresVerification:
                    return JsonConvert.SerializeObject(new { error = true, errorMessage = "accountHasToBeVerified" });
                case SignInStatus.Failure:
                default:
                    return JsonConvert.SerializeObject(new { error = true, errorMessage = "invalidLoginAttempt" });
            }
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public string PostAutoLogin(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                logger.Error("Model Invalid");
                return JsonConvert.SerializeObject(new { error = true, notification = "AutoLoginInvalidToken" });
            }
            //Check code
            if(!UserService.verifyCodeForAutoLogin(value))
                return JsonConvert.SerializeObject(new { error = true, notification = "AutoLoginInvalidToken" });

            var user = UserService.getUserFromCode(value);            
            SignInManager.SignIn(user,true,true);
            return JsonConvert.SerializeObject(new { error = false, notification = "AutoLoginSuccess" });          
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }
               
        //
        // POST: /Account/postRegister
        [HttpPost]     
        [AllowAnonymous]
        public string postRegister(RegisterViewModel model)
        {
            if(!Convert.ToBoolean(ConfigurationManager.AppSettings["AutoRegistration:Enabled"]))
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "auto-rgeistration-disabled" });

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = UserManager.CreateAsync(user, model.Password).Result;
                if (result.Succeeded)
                {
                    //create space and add space to user
                    Space space = new Space();
                    MySqlIdentityDbContext db = new MySqlIdentityDbContext();
                    db.Spaces.Add(space);
                    db.SaveChanges();
                    var addClaimResult = UserManager.AddClaimAsync(UserManager.FindByEmail(user.Email).Id, new Claim("SpaceId", space.Id)).Result;

                    // Add privilèges via role specify in the configuration to the user    
                    addUserInRole(user);
                       
                    // Send an email with the link to confirm email async
                    if (Convert.ToBoolean(ConfigurationManager.AppSettings["AutoRegistration:ConfirmationEmail:Enabled"]))
                    {
                        string code = UserManager.GenerateEmailConfirmationTokenAsync(user.Id).Result;
                        Task.Run(() => sendConfirmationEmail(user, code));
                    }

                    //Log in user
                    SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    return JsonConvert.SerializeObject(new { error = false, user = user });
                }
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "creationIssue" });
            }

            // If we got this far, something failed, redisplay form
            return JsonConvert.SerializeObject(new { error = true, errorMessage = "dataIssue" });
        }

        private void addUserInRole(ApplicationUser user)
        {
            try {
                string role = ConfigurationManager.AppSettings["AutoRegistration:UserRoleDefault:Name"];
                if (!string.IsNullOrEmpty(role))
                {
                    string roleJson = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/App_Data/roles.json"));
                    JObject roles = JObject.Parse(roleJson);
                    JArray arrayOfRoles = (JArray)roles["roles"];
                    foreach (JObject r in arrayOfRoles)
                    {
                        if (r["name"].ToString() == role)
                        {
                            foreach (var privilege in r["privileges"])
                            {
                                UserManager.AddToRole(user.Id, privilege.ToString());
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error("the user : " + user.UserName + " has not been associated to the default role - error :" + ex.Message );
            }
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        public string postForgotPassword(ForgotPasswordViewModel model)
        {
            try
            {
                if (!Convert.ToBoolean(ConfigurationManager.AppSettings["ForgotPassword:Enabled"]))
                    return JsonConvert.SerializeObject(new { error = true, errorMessage = "forgotPasswordNotActivated" });

                if (ModelState.IsValid)
                {
                    var user = UserManager.FindByName(model.Email);
                    if (user == null)
                    {
                        return JsonConvert.SerializeObject(new { error = true, errorMessage = "dataIssue" });
                    }

                    string code = UserManager.GeneratePasswordResetToken(user.Id);
                    if (sendForgotPasswordEmail(user, code))
                        return JsonConvert.SerializeObject(new { error = false }); //SUCCESS
                    else
                        return JsonConvert.SerializeObject(new { error = true, errorMessage = "dataIssue" });
                }

                // If we got this far, something failed, redisplay form
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "dataIssue" });
            }
            catch(Exception ex)
            {
                logger.Error(ex.Message);
                return JsonConvert.SerializeObject(new { error = true, notification = "ServerError" });
            }
        }

        //
        // POST: /Account/postChangePasswordForgot
        [HttpPost]
        [AllowAnonymous]
        public string postChangePasswordForgot(ChangePasswordModel model)
        {            
            if (!Convert.ToBoolean(ConfigurationManager.AppSettings["ForgotPassword:Enabled"]))
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "forgotPasswordNotActivated" });

            if (ModelState.IsValid)
            {
                var user = UserManager.FindById(model.UserId);
                if (user == null)
                {
                    return JsonConvert.SerializeObject(new { error = true, errorMessage = "dataIssue" });
                }
                model.Code = model.Code.Replace(" ", "+");
                var result = UserManager.ResetPassword(user.Id,model.Code,model.Password);
                if (result.Succeeded)
                    return JsonConvert.SerializeObject(new { error = false }); //SUCCESS
                else
                    return JsonConvert.SerializeObject(new { error = true, errorMessage = "dataIssue" });
            }

            // If we got this far, something failed, redisplay form
            return JsonConvert.SerializeObject(new { error = true, errorMessage = "dataIssue" });
        }

        //
        // POST: /Account/postChangePassword
        [HttpPost]
        public string postChangePassword(string value)
        {
            try {
                JObject obj = JObject.Parse(value);
                var currentPassword = obj["currentpassword"].ToString();
                var newPassword = obj["newpassword"].ToString();
                
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (user == null)
                {
                    return JsonConvert.SerializeObject(new { error = true, errorMessage = "dataIssue" });
                }
                var result = UserManager.ChangePassword(user.Id, currentPassword, newPassword);
                if (result.Succeeded)
                    return JsonConvert.SerializeObject(new { error = false }); //SUCCESS
                else
                    return JsonConvert.SerializeObject(new { error = true, errorMessage = "dataIssue" });              
            }
            catch(Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "dataIssue" });
            }
        }

        //
        // GET: /Account/ConfirmEmail
        [HttpPost]
        [AllowAnonymous]
        public string PostVerifyEmail(string userId, string code)
        {
            if (!Convert.ToBoolean(ConfigurationManager.AppSettings["AutoRegistration:ConfirmationEmail:Enabled"]))
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "autoRegistrationDisabled" });

            if (userId == null || code == null)
            {
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "dataIssue" });
            }
            code = code.Replace(" ", "+");
            var result = UserManager.ConfirmEmailAsync(userId, code).Result;
            if(result.Succeeded)
                return JsonConvert.SerializeObject(new { error = false });
            else
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "dataIssue" });
        }

        #region helpers api
        private void sendConfirmationEmail(ApplicationUser user, string code)
        {
            try
            {
                string baseUrl = Request.Url.Authority;
                if (Request.Url.Authority.Contains("localhost"))
                    baseUrl = "http://example.com";

                string callbackUrl = baseUrl + "/Angular/index.html#verify_email" + "?userid=" + user.Id + "&code=" + code;               

                EmailFactory factory = new EmailFactory();
                MailMessage message = new MailMessage();                
                message.To.Add(new MailAddress(user.Email));
                message.IsBodyHtml = true;
                if(HostingEnvironment.MapPath("~/Angular/email_templates/email_verification.html") != null)
                    message.Body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/Angular/email_templates/email_verification.html"));
                //Dev mode
                else
                    message.Body = System.IO.File.ReadAllText(@"D:\Travail\Reta\RetaTrunk\Reta\Reta\Angular\email_templates\email_verification.html");


                // embed logo if exist
                string path = getPathToLogoForUser(user);
                if (!string.IsNullOrEmpty(path))
                {
                    Attachment inlineLogo = new Attachment(path);
                    message.Attachments.Add(inlineLogo);
                    string contentID = "Logo";
                    inlineLogo.ContentId = contentID;
                    //To make the image display as inline and not as attachment
                    inlineLogo.ContentDisposition.Inline = true;
                    inlineLogo.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
                    string urlToTemplateFile = "cid:" + contentID;
                    message.Body = message.Body.Replace("##urlToTemplateFolder##", urlToTemplateFile);
                }
                else
                {
                    message.Body = message.Body.Replace("##urlToTemplateFolder##", "");
                }

                message.Body = message.Body.Replace("##urlToConfirmPage##", callbackUrl);
                message.Subject = "Email Verification";
                message.From = new MailAddress(ConfigurationManager.AppSettings["AutoRegistration:ConfirmationEmail:DefaultFrom"]);
                factory.Send(message); 
                logger.Info("Email confimation sent to : " + user.Email);
            }
            catch(Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        private bool sendForgotPasswordEmail(ApplicationUser user, string code)
        {
            try
            {
                string baseUrl = Request.Url.Authority;
                // For dev
                if (Request.Url.Authority.Contains("localhost"))
                    baseUrl = "http://vps232338.ovh.net";

                string callbackUrl = baseUrl + "/Angular/index.html#change_password" + "?userid=" + user.Id + "&code=" + code;

                EmailFactory factory = new EmailFactory();
                MailMessage message = new MailMessage();
                message.To.Add(new MailAddress(user.Email));
                message.IsBodyHtml = true;
                if (HostingEnvironment.MapPath("~/Angular/email_templates/email_forgot_password.html") != null)
                    message.Body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/Angular/email_templates/email_forgot_password.html"));
                //Dev mode
                else
                    message.Body = System.IO.File.ReadAllText(@"D:\Travail\Reta\RetaTrunk\Reta\Reta\Angular\email_templates\email_forgot_password.html");

                // embed logo if exist
                string path = getPathToLogoForUser(user);
                if (!string.IsNullOrEmpty(path))
                {
                    Attachment inlineLogo = new Attachment(path);
                    message.Attachments.Add(inlineLogo);                  
                    string contentID = "Logo";
                    inlineLogo.ContentId = contentID;
                    //To make the image display as inline and not as attachment
                    inlineLogo.ContentDisposition.Inline = true;
                    inlineLogo.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
                    string urlToTemplateFile = "cid:" + contentID;
                    message.Body = message.Body.Replace("##urlToTemplateFolder##", urlToTemplateFile);
                }
                else
                {
                    message.Body = message.Body.Replace("##urlToTemplateFolder##", "");
                }                
            
                message.Body = message.Body.Replace("##urlToConfirmPage##", callbackUrl);
                message.Subject = "Forgot Password";
                message.From = new MailAddress(ConfigurationManager.AppSettings["ForgotPassword:Email:DefaultFrom"]);
                factory.Send(message);
                logger.Info("Forgot password sent to : " + user.Email);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        private string getPathToLogoForUser(ApplicationUser user)
        {
            var claims = UserManager.GetClaims(UserManager.FindByEmail(user.Email).Id);
            if(claims == null)
                return "";
            var spaceClaim = claims.Where(c => c.Type == "SpaceId").FirstOrDefault();
            if (spaceClaim == null)
                return "";

            var space = db.Spaces.Find(spaceClaim.Value);
            if(space.Configuration == null)
                return "";
            var spaceLogoUrl = JObject.Parse(space.Configuration)["spaceLogoUrl"];
            if (spaceLogoUrl == null)
                return "";
            string path = HostingEnvironment.MapPath(spaceLogoUrl.ToString());

            return path;          
        }
             
        #endregion 
        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/logOut
        [HttpPost]       
        public string logOut()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return JsonConvert.SerializeObject(new { error = false, result = "" });
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}