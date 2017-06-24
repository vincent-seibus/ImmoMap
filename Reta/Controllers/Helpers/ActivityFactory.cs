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
using System.Threading.Tasks;
using System.Web;

namespace Reta.Controllers
{
    public static class ActivityFactory
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ActivityFactory));
               
        public static void SetActivity(string UserId)
        {
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
                ApplicationUserManager UserManager = new ApplicationUserManager(store);
                // Add last activity of the current user
                var claims = UserManager.GetClaims(UserId);

                var lastActivityDate = claims.Where(a => a.Type == "LastActivityDate").FirstOrDefault();                        
                
                if (lastActivityDate != null)
                {
                    UserManager.RemoveClaim(UserId, lastActivityDate);
                }

                UserManager.AddClaim(UserId, new Claim("LastActivityDate", JsonConvert.SerializeObject(DateTime.UtcNow)));
            }
            catch(Exception ex)
            {
                logger.Error(ex.Message);
            }
            
        }

        public static Task SetActivityAsync(string UserId)
        {
            return Task.Run(() => SetActivity(UserId));
        }

        public static string SetMetadata(string userName, string value, string method,string listMetadata)
        {
            try
            {

                JObject obj = JObject.Parse(value);
                obj["Metadata"] = "";

                var listMetadataDeserialised = new List<Metadata>();
                if (listMetadata != null)
                     listMetadataDeserialised = JsonConvert.DeserializeObject<List<Metadata>>(listMetadata);

                Metadata metadata = new Metadata()
                {
                    who = userName,
                    what = obj.ToString(),
                    when = DateTime.UtcNow,
                    how = method
                };  

                listMetadataDeserialised.Add(metadata);

                
                var listMetadataSerialized = JsonConvert.SerializeObject(listMetadataDeserialised.OrderByDescending(a => a.when));
                var countByte = System.Text.ASCIIEncoding.Unicode.GetByteCount(listMetadataSerialized);
                int reduce = 1;
                while (countByte > 56000)
                {
                    listMetadataSerialized = JsonConvert.SerializeObject(listMetadataDeserialised.OrderByDescending(a => a.when).Take(listMetadataDeserialised.Count - reduce));
                    countByte = System.Text.ASCIIEncoding.Unicode.GetByteCount(listMetadataSerialized);
                    reduce++;
                } 

                return listMetadataSerialized;
            }
            catch(Exception ex)
            {
                logger.Error(ex.Message);
                return listMetadata;
            }
        }

    }
}