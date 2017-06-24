using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reta.Controllers.Helpers;
using Reta.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Reta.Controllers
{
    [Authorize(Roles = "Supervisor,viewSpaceConfiguration")]
    public class SpacesController : Controller
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(SpacesController));
        private MySqlIdentityDbContext db = new MySqlIdentityDbContext();
        // GET: Spaces
        public string GetSpaceForCurrentUser()
        {
            SpaceService spaceService = new SpaceService(User.Identity.GetUserId());
            if (spaceService.Space != null)
                return JsonConvert.SerializeObject(spaceService.Space);   
            return "";
        }

        [Authorize(Roles = "Supervisor")]
        public string GetSpace(string value)
        {
            var space = db.Spaces.Find(value);
            return JsonConvert.SerializeObject(space);
        }

        [Authorize(Roles = "Supervisor")]
        public string GetSpaces()
        {
            return JsonConvert.SerializeObject(db.Spaces);
        }

        [HttpPost]
        public string PostSpaceForCurrentUser(string value)
        {
            JObject obj = JObject.Parse(value);
            
            var Name = obj["Name"];
            var Configuration = obj["Configuration"];

            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);
            var claims = UserManager.GetClaims(User.Identity.GetUserId());
            var claim = claims.Where(a => a.Type == "SpaceId").FirstOrDefault();

            if (claim != null)
            {
                var space = db.Spaces.Find(claim.Value);
                if (space == null)
                {
                    logger.Info("NoSpaceFoundInDb - User : " + User.Identity.GetUserName());
                    return JsonConvert.SerializeObject(new { error = true, errorMessage = "NoSpaceFoundInDb" });
                }

                DesignManager designManager = new DesignManager();
                // Create css file specific for this space with the colour provided by the user async
                designManager.CreateUpdateCssFileForSpace(space.Id, designManager.getColourDesignOfSpace(Configuration.ToString()));

                space.Name = Name.ToString();
                space.Configuration = Configuration.ToString();
                space.Metadata = ActivityFactory.SetMetadata(User.Identity.GetUserName(), value, "PostSpaceForCurrentUser", space.Metadata);

                db.Entry(space).State = EntityState.Modified;
                db.SaveChanges();

                return JsonConvert.SerializeObject(new { error = false, space = space });
            }

            logger.Info("NoSpaceFoundInUserConfig - User : " + User.Identity.GetUserName());
            return JsonConvert.SerializeObject(new { error = true, errorMessage = "NoSpaceFoundInUserConfig" });
        }
        
        [Authorize(Roles = "Supervisor")]
        [HttpPost]
        public string PostSpace(string value)
        {
            JObject obj = JObject.Parse(value);

            var Id = obj["Id"];
            var Name = obj["Name"];
            var Configuration = obj["Configuration"];

            if (Id != null && !string.IsNullOrEmpty(Id.ToString()))
            {
                var space = db.Spaces.Find(Id.ToString());
                if (space == null)
                {
                    logger.Info("NoSpaceFoundInDb - User : " + User.Identity.GetUserName());
                    return JsonConvert.SerializeObject(new { error = true, errorMessage = "NoSpaceFoundInDb" });
                }

                DesignManager designManager = new DesignManager();
                // Create css file specific for this space with the colour provided by the user async
                designManager.CreateUpdateCssFileForSpace(space.Id, designManager.getColourDesignOfSpace(Configuration.ToString()));

                space.Name = Name.ToString();
                space.Configuration = Configuration.ToString();
                space.Metadata = ActivityFactory.SetMetadata(User.Identity.GetUserName(), value, "PostSpace", space.Metadata);

                db.Entry(space).State = EntityState.Modified;
                db.SaveChanges();

                return JsonConvert.SerializeObject(new { error = false, space = space });
            }
            else
            {
                var space = new Space();
                space.Name = Name.ToString();
                space.Configuration = Configuration.ToString();
                space.Metadata = ActivityFactory.SetMetadata(User.Identity.GetUserName(), value, "PostSpace", space.Metadata);

                db.Spaces.Add(space);
                db.SaveChanges();
                return JsonConvert.SerializeObject(new { error = false, space = space });
            }
        }
       
        [HttpPost]
        public string UploadSpaceLogo()
        {            
            try
            {
                string result = "";
                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        // get a stream
                        var stream = fileContent.InputStream;
                        // and optionally write the file to disk
                        var fileName = Path.GetFileName(file);
                        var fileNameToRecord = DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "-" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond + "-" + fileName;
                        //Create directory if not exist
                        if (!Directory.Exists(HostingEnvironment.MapPath("~/Files/SpacesLogo")))
                            Directory.CreateDirectory(HostingEnvironment.MapPath("~/Files/SpacesLogo"));

                        var path = Path.Combine(HostingEnvironment.MapPath("~/Files/SpacesLogo"), fileNameToRecord);

                        using (var fileStream = System.IO.File.Create(path))
                        {
                            stream.CopyTo(fileStream);
                        }
                        
                        result = "/Files/SpacesLogo/" + fileNameToRecord;  
                    }
                }

                return JsonConvert.SerializeObject(new { error = false, result = result });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "UploadFailed" });
            }

        }

        [HttpPost]
        public string DeleteSpace(string value)
        {
            JObject obj = JObject.Parse(value);

            var Id = obj["Id"];
            var Name = obj["Name"];
            var Configuration = obj["Configuration"];

            if (Id != null && !string.IsNullOrEmpty(Id.ToString()))
            {
                var space = db.Spaces.Find(Id.ToString());
                if (space == null)
                {
                    logger.Info("NoSpaceFoundInDb - User : " + User.Identity.GetUserName());
                    return JsonConvert.SerializeObject(new { error = true, errorMessage = "NoSpaceFoundInDb" });
                }

                db.Spaces.Remove(space);
                db.SaveChanges();
                return JsonConvert.SerializeObject(new { error = false, space = space });
            }
            return JsonConvert.SerializeObject(new { error = true, errorMessage = "NoSpaceId" });
        }

        [HttpGet]
        public string GetSpaceDirectorySizeByUser()
        {
            var sizeInBytes = DocumentService.SpaceDirectorySizeWithUser(User.Identity.GetUserId());
            var sizeInKBytes = Math.Round((double)(sizeInBytes / 1000),3);
            var sizeInMBytes = Math.Round((double)(sizeInKBytes / 1000),3);
            var sizeInGBytes = Math.Round((double)(sizeInMBytes / 1000),3);
            return JsonConvert.SerializeObject(new { error = false, sizeInBytes = sizeInBytes, sizeInKBytes = sizeInKBytes, sizeInMBytes = sizeInMBytes, sizeInGBytes = sizeInGBytes });
        }
             
    }
}