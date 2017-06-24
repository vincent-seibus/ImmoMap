using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reta.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Hosting;

namespace Reta.Controllers.Helpers
{
    public class EmailFactory
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(EmailFactory));
        private MySqlIdentityDbContext db = new MySqlIdentityDbContext();
        private SmtpClient client { get; set; }
        private NameValueCollection Configuration { get; set; }
        private SmtpConfigurationUser smtpConfig { get; set; }

        public EmailFactory()
        {
            Configuration = ConfigurationManager.AppSettings;

            try
            {
                client = new SmtpClient();
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(Configuration["SmtpServer:Username"], Configuration["SmtpServer:Password"]);
                client.EnableSsl = Convert.ToBoolean(Configuration["SmtpServer:EnableSsl"]);

                client.Host = Configuration["SmtpServer:Host"];
                client.Port = Convert.ToInt32(Configuration["SmtpServer:Port"]);
                client.Timeout = Convert.ToInt32(Configuration["SmtpServer:Timeout"]);

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        public EmailFactory(IList<Claim> claims)
        {
            Configuration = ConfigurationManager.AppSettings;
            var smtpConfigJson = claims.Where(a => a.Type == "SmtpConfiguration").FirstOrDefault();

            if (smtpConfigJson == null)
            {
                logger.Error("No email SMTP Configuration");
                return;
            }
                      
            try
            {
                smtpConfig = JsonConvert.DeserializeObject<SmtpConfigurationUser>(smtpConfigJson.Value);
                client = new SmtpClient();
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(smtpConfig.username, smtpConfig.password);
                client.EnableSsl = Convert.ToBoolean(smtpConfig.enableSsl);

                client.Host = smtpConfig.host;
                client.Port = Convert.ToInt32(smtpConfig.port);
                client.Timeout = Convert.ToInt32(smtpConfig.timeout);

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        public bool Send(MailMessage Message)
        {
            client.Send(Message);
            return true;
        }

        public MailMessage GetTemplate(string TemplateId, string SendTo, string SendFrom = null)
        {
            // get the template email
            MailMessage Message = new MailMessage();

            // default from if no default
            if (!string.IsNullOrEmpty(SendFrom))
                Message.From = new MailAddress(SendFrom);
            else if (smtpConfig != null)
                Message.From = new MailAddress(smtpConfig.username); 
            else
                Message.From = new MailAddress(Configuration["SmtpServer:DefaultFrom"]);

            // add the recipient
            Message.To.Add(new MailAddress(SendTo));

            // Récupérer un template d'email en base de données           
            EmailTemplate bodyAndSubject = GetEmailFromTemplateId(TemplateId);
            if (bodyAndSubject == null)
                return null;           

            Message.Subject = bodyAndSubject.Subject;
            Message.IsBodyHtml = true;
            Message.Body = bodyAndSubject.Body;

            return Message;
        }

        public MailMessage ReplaceVariableInTemplate(MailMessage message, object obj)
        {

            foreach (var prop in obj.GetType().GetProperties())
            {
                string propertyNameString = "##" + prop.Name + "##";
                string propertyValueString = "";

                if (prop.GetValue(obj, null) == null)
                    propertyValueString = "";
                else
                    propertyValueString = prop.GetValue(obj, null).ToString();

                if (message.Body.IndexOf(propertyNameString) < 0)
                    continue;

                message.Body = message.Body.Replace(propertyNameString, propertyValueString);
            }

            return message;
        }

        public MailMessage GetTemplateStandard(EmailType type, string SendTo, Language lang, string SendFrom = null)
        {
            // get the template email
            MailMessage Message = new MailMessage();

            // default from if no default
            if (!string.IsNullOrEmpty(SendFrom))
                Message.From = new MailAddress(SendFrom);
            else if (smtpConfig != null)
                Message.From = new MailAddress(smtpConfig.username);
            else
                Message.From = new MailAddress(Configuration["SmtpServer:DefaultFrom"]);
            // add the recipient
            Message.To.Add(new MailAddress(SendTo));

            // Récupérer un template d'email standard       
            EmailTemplate bodyAndSubject = GetEmailFromStandardFile(type, lang);
            if (bodyAndSubject == null)
                return null;

            Message.Subject = bodyAndSubject.Subject;
            Message.IsBodyHtml = true;
            Message.Body = bodyAndSubject.Body;

            return Message;

        }

        #region helpers

        private EmailTemplate GetEmailFromTemplateId(string TemplateId)
        {
            var template = db.EmailTemplates.Where(a => a.Id == TemplateId).FirstOrDefault();
            return template;           
        }

        private EmailTemplate GetEmailFromStandardFile(EmailType type, Language lang)
        {            
            var emailTemplates = JsonConvert.DeserializeObject<List<EmailTemplate>>(File.ReadAllText(HostingEnvironment.MapPath("~/App_Data/emailtemplates.json")));
            EmailTemplate template = emailTemplates.Where(a => a.Type == type.ToString() && a.Lang == lang.ToString()).FirstOrDefault();
            template.Body = File.ReadAllText(HostingEnvironment.MapPath(template.BodyUrl));
            return template;
        }

        private string getPathToLogoForUser(ApplicationUser user)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);
            var claims = UserManager.GetClaims(UserManager.FindByEmail(user.Email).Id);
            if (claims == null)
                return "";
            var spaceClaim = claims.Where(c => c.Type == "SpaceId").FirstOrDefault();
            if (spaceClaim == null)
                return "";

            var space = db.Spaces.Find(spaceClaim.Value);
            if (space.Configuration == null)
                return "";
            var spaceLogoUrl = JObject.Parse(space.Configuration)["spaceLogoUrl"];
            if (spaceLogoUrl == null)
                return "";
            string path = HostingEnvironment.MapPath(spaceLogoUrl.ToString());

            return path;
        }

        public MailMessage EmbedLogoIfExist(ApplicationUser user, MailMessage message) {
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
                message.Body = message.Body.Replace("##imagelogo##", urlToTemplateFile);
            }
            else
            {
                message.Body = message.Body.Replace("##imagelogo##", "");
            }
            return message;
        }

        #endregion
    }

    public enum EmailType
    {
        ForgotPassword,
        VerifyEmail,
        InvitationQuiz,
    }
      
}