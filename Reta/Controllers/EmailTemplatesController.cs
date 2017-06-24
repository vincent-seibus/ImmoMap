using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Reta.Models;
using Reta.Controllers.Helpers;
using Newtonsoft.Json;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using log4net;
using System.Configuration;
using System.Net.Mail;

namespace Reta.Controllers
{
    [Authorize]
    public class EmailTemplatesController : Controller
    {
        private MySqlIdentityDbContext db = new MySqlIdentityDbContext();
        private static readonly ILog logger = LogManager.GetLogger(typeof(EmailTemplatesController));

        private ApplicationUserManager _userManager;

        public EmailTemplatesController()
        {
        }

        public EmailTemplatesController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // GET: EmailTemplates
        public ActionResult Index()
        {
            return View(db.EmailTemplates.ToList());
        }

        // GET: EmailTemplates
        public string GetEmailTemplates()
        {
            return JsonConvert.SerializeObject(db.EmailTemplates.ToList());
        }

        // POST : CReate EmailTemplates
        [HttpPost]
        public string PostCreateTemplate(string value)
        {
            try
            {
                EmailTemplate template = JsonConvert.DeserializeObject<EmailTemplate>(value);
                SpaceService spaceService = new SpaceService();
                template.SpaceId = spaceService.GetSpaceIdFromUser(User.Identity.GetUserId());
                db.EmailTemplates.Add(template);
                db.SaveChanges();
                return JsonConvert.SerializeObject(new { error = false, template = template });
            }
            catch(Exception ex)
            {
                logger.Error(ex.Message);
                return JsonConvert.SerializeObject(new { error = true, errorMessage = ex.Message });
            }
        }

        // POST : Edit EmailTemplates
        [HttpPost]
        public string PostEditTemplate(string value)
        {
            try
            {
                EmailTemplate template = JsonConvert.DeserializeObject<EmailTemplate>(value);
                db.Entry(template).State = EntityState.Modified;
                db.SaveChanges();
                return JsonConvert.SerializeObject(new { error = false, template = template });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return JsonConvert.SerializeObject(new { error = true, errorMessage = ex.Message });
            }
        }

        // POST : Delete EmailTemplates
        [HttpPost]
        public string PostDeleteTemplate(string value)
        {
            try
            {
                EmailTemplate template = JsonConvert.DeserializeObject<EmailTemplate>(value);
                EmailTemplate emailTemplate = db.EmailTemplates.Find(template.Id);
                db.EmailTemplates.Remove(emailTemplate);
                db.SaveChanges();             
                return JsonConvert.SerializeObject(new { error = false, template = "" });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return JsonConvert.SerializeObject(new { error = true, errorMessage = ex.Message });
            }
        }

        // POST: Send Emails       
        [HttpPost]
        public string Send(string value)
        {
            EmailToSend emailToSend = JsonConvert.DeserializeObject<EmailToSend>(value);
            MySqlIdentityDbContext db = new MySqlIdentityDbContext();
            var files = Request.Files;

            string result = "";

            if(User == null || !User.Identity.IsAuthenticated)
            {
                return "";
            }

            var claims = UserManager.GetClaims(User.Identity.GetUserId());
            EmailFactory factory = new EmailFactory(claims);
            int j = 0;
            foreach (string sendTo in emailToSend.SendTo)
            {
                try
                {
                    var message = factory.GetTemplate(emailToSend.Template, sendTo);
                    Object obj = new Object();

                    /*/
                    if (emailToSend.Type == "candidat")
                    {
                        obj = db.Candidats.Where(a => a.Email == sendTo).FirstOrDefault();
                    }
                    else if (emailToSend.Type == "customer")
                    {
                        obj = db.Customers.Where(a => a.Email == sendTo).FirstOrDefault();
                    }
                    else if (emailToSend.Type == "linkedin")
                    {
                        obj = db.ContactLinkedins.Where(a => a.Email == sendTo).FirstOrDefault();
                    }
                    else
                    {
                        result = result + "<tr><td>" + sendTo + "</td><td><b> Type d'objet non spécifié </b> </td></tr>";
                        continue;
                    }/*/

                    // ajout des pièces jointe si elles existent
                    if (files.Count > 0)
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            message.Attachments.Add(new System.Net.Mail.Attachment(files[i].InputStream, files[i].FileName));
                        }                        
                    }

                    // Envoyer l'email à l'administrateur et à l'utilisateur qui envoie 
                    if (j == 0)
                    {
                        try
                        {
                            message.To.Clear();
                            message.To.Add(new MailAddress(User.Identity.Name));
                            factory.Send(factory.ReplaceVariableInTemplate(message, obj));
                            logger.Info(" Email copy sent to : " + User.Identity.Name);
                            if (ConfigurationManager.AppSettings["SmtpServer:AllEmailCciTo"] != User.Identity.Name)
                            {
                                message.To.Clear();
                                message.To.Add(new MailAddress(ConfigurationManager.AppSettings["SmtpServer:AllEmailCciTo"]));
                                factory.Send(factory.ReplaceVariableInTemplate(message, obj));
                                logger.Info(" Email copy sent to : " + ConfigurationManager.AppSettings["SmtpServer:AllEmailCciTo"]);
                            }

                            if (ConfigurationManager.AppSettings["SmtpServer:AllEmailCciTo2"] != User.Identity.Name)
                            {
                                message.To.Clear();
                                message.To.Add(new MailAddress(ConfigurationManager.AppSettings["SmtpServer:AllEmailCciTo2"]));
                                factory.Send(factory.ReplaceVariableInTemplate(message, obj));
                                logger.Info(" Email copy sent to : " + ConfigurationManager.AppSettings["SmtpServer:AllEmailCciTo2"]);
                            }
                        }
                        catch(Exception ex)
                        {
                            logger.Error(" Error in sending Cci email - " + ex.Message);
                        }

                        message.To.Clear();
                        message.To.Add(new MailAddress(sendTo));
                    }

                    factory.Send(factory.ReplaceVariableInTemplate(message, obj));
                    result = result + "<tr><td>" + sendTo + "</td><td><b> Envoyé </b> </td></tr>";
                    logger.Info(" Email copy sent to : " + sendTo);
                    j++;
                }
                catch(Exception ex)
                {
                    result = result + "<tr><td>" + sendTo + "</td><td><b> Non envoyé</b> </td></tr>";
                    logger.Error(ex.Message);
                }
            }
            
            return result;
        }
               

        // GET: EmailTemplates/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmailTemplate emailTemplate = db.EmailTemplates.Find(id);
            if (emailTemplate == null)
            {
                return HttpNotFound();
            }
            return View(emailTemplate);
        }

        // GET: EmailTemplates/Create        
        public ActionResult Create()
        {
            return View();
        }

        // POST: EmailTemplates/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,Type,Subject,Body,DynamicField")] EmailTemplate emailTemplate)
        {
            emailTemplate.Id = Guid.NewGuid().ToString();
            if (ModelState.IsValid)
            {
                db.EmailTemplates.Add(emailTemplate);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(emailTemplate);
        }

        // GET: EmailTemplates/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmailTemplate emailTemplate = db.EmailTemplates.Find(id);
            if (emailTemplate == null)
            {
                return HttpNotFound();
            }
            return View(emailTemplate);
        }

        // POST: EmailTemplates/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,Type,Subject,Body,DynamicField")] EmailTemplate emailTemplate)
        {
            if (ModelState.IsValid)
            {
                db.Entry(emailTemplate).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(emailTemplate);
        }

        // GET: EmailTemplates/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmailTemplate emailTemplate = db.EmailTemplates.Find(id);
            if (emailTemplate == null)
            {
                return HttpNotFound();
            }
            return View(emailTemplate);
        }

        // POST: EmailTemplates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            EmailTemplate emailTemplate = db.EmailTemplates.Find(id);
            db.EmailTemplates.Remove(emailTemplate);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    internal class EmailToSend
    {
        public string Template { get; set; }
        public List<string> SendTo { get; set; }
        public string Type { get; set; }
    }
}
