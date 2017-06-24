using log4net;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Reta.Controllers.Helpers;
using Reta.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Reta.Controllers
{
    [Authorize(Roles = "Supervisor,ViewBuildings,ModifyBuildings")]
    public class BuildingsController : Controller
    {
        private MySqlIdentityDbContext db = new MySqlIdentityDbContext();
        private static readonly ILog logger = LogManager.GetLogger(typeof(BuildingsController));

        // GET API : Buildings 
        [Authorize(Roles = "Supervisor,ViewBuildings")]
        [HttpGet]
        public string GetBuildings()
        {
            try
            {
                //set activity user async                
                ActivityFactory.SetActivityAsync(User.Identity.GetUserId());

                var Buildings = new List<Building>();
                SpaceService service = new SpaceService(User.Identity.GetUserId());
                Buildings = db.Buildings.Where(a => a.DeletedOn == null && a.SpaceId == service.Space.Id).OrderByDescending(a => a.CreatedOn).ToList();
               

                var BuildingsSerialized = JsonConvert.SerializeObject(new { data = Buildings },
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                return BuildingsSerialized;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "ServerError" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Supervisor,ModifyBuildings")]
        public string PostBuilding(string value)
        {
            try
            {
                //set activity user async                
                ActivityFactory.SetActivityAsync(User.Identity.GetUserId());
                var building = JsonConvert.DeserializeObject<Building>(value);
                SpaceService service = new SpaceService(User.Identity.GetUserId());
                if (!db.Buildings.Where(b => b.Id == building.Id).Any())
                {
                    building = (Building)EntityService.OnCreateEntity(building,User.Identity.GetUserId());
                    db.Buildings.Add(building);
                    db.SaveChanges();
                    return JsonConvert.SerializeObject(new { error = false, data = building });
                }
                else
                {
                    db.Entry(building).State = EntityState.Modified;
                    db.SaveChanges();
                    return JsonConvert.SerializeObject(new { error = false, data = building });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return JsonConvert.SerializeObject(new { error = true, errorMessage = "ServerError" });
            }

        }

        [HttpPost]
        [Authorize(Roles = "Supervisor,ModifyBuildings")]
        public string PostDeleteBuilding(string value)
        {
            try
            {
                ActivityFactory.SetActivityAsync(User.Identity.GetUserId());
                if (value != null && !string.IsNullOrEmpty(value))
                {
                    var building = db.Buildings.Find(value);
                    if (building == null)
                        return JsonConvert.SerializeObject(new { error = true, notification = "EntityNotFoundNotif" });

                    building.Metadata = ActivityFactory.SetMetadata(User.Identity.GetUserName(), value, "PostDeleteBuilding", building.Metadata);
                    building = (Building)EntityService.OnDeleteEntity(building, User.Identity.GetUserId());
                    db.Entry(building).State = EntityState.Modified;
                    db.SaveChanges();
                    return JsonConvert.SerializeObject(new { error = false, notification = "EntityDeletedSuccessNotif" });
                }
                return JsonConvert.SerializeObject(new { error = true, notification = "NoEntityIdNotif" });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return JsonConvert.SerializeObject(new { error = true, notification = "ServerError" });
            }

        }

        [HttpPost]
        [Authorize(Roles = "Supervisor,ModifyBuildings")]
        public string PostDeleteMultiBuilding(string value)
        {
            try
            {
                ActivityFactory.SetActivityAsync(User.Identity.GetUserId());
                var buildingids = JsonConvert.DeserializeObject<List<string>>(value);
                foreach (var id in buildingids)
                {
                    var building = db.Buildings.Find(id);
                    if (building != null)
                    {
                        EntityService.OnDeleteEntity(building, User.Identity.GetUserId());
                        building.Metadata = ActivityFactory.SetMetadata(User.Identity.GetUserName(), value, "PostDeleteMultiBuilding", building.Metadata);
                    }
                }
                db.SaveChanges();
                return JsonConvert.SerializeObject(new { error = false, notification = "DeleteMultiNotifSuccess", response = "" });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return JsonConvert.SerializeObject(new { error = true, notification = "ServerError" });
            }
        }
    }
}