using log4net;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Reta.Controllers.Helpers;
using Reta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Reta.Models.Helpers;

namespace Reta.Controllers
{
    [Authorize(Roles = "Supervisor,ModifyTrash,ViewTrash")]
    public class TrashController : Controller
    {
        private MySqlIdentityDbContext db = new MySqlIdentityDbContext();
        private static readonly ILog logger = LogManager.GetLogger(typeof(TrashController));
        [HttpGet]
        public string GetDeletedItems()
        {
            try
            {
                SpaceService service = new SpaceService(User.Identity.GetUserId());
                List<Entity> list = new List<Entity>();
                List<IEntity> listToTransform = new List<IEntity>();

                var buildings = db.Buildings.Where(a => a.DeletedOn != null && a.SpaceId == service.Space.Id).ToList();
                listToTransform.AddRange(buildings);
               
                foreach (var item in listToTransform)
                {
                    list.Add(EntityService.CastIEntityToEntity(item));
                }               
                return JsonConvert.SerializeObject(new { error = false, data = list.OrderByDescending(a => a.DeletedOn).ToList(), notification = "trashItemsRetrievedSuccess" });
            }
            catch(Exception ex)
            {
                logger.Error(ex.Message);
                return JsonConvert.SerializeObject(new { error = true, notification = "trashItemsError" });
            }
           
        }
        [HttpPost]
        [Authorize(Roles = "Supervisor,ModifyTrash")]
        public string PostRestore(string value)
        {
            try
            {
                var entities = JsonConvert.DeserializeObject<List<Entity>>(value);
                foreach (var entity in entities)
                {
                    var entityModified = EntityService.findEntity(entity);
                    entityModified = EntityService.OnRestoreEntity(entityModified);
                    entityModified.Metadata = ActivityFactory.SetMetadata(User.Identity.GetUserName(), value, "PostRestore", entityModified.Metadata);
                    db.Entry(entityModified).State = EntityState.Modified;
                }
                db.SaveChanges();
                return JsonConvert.SerializeObject(new { error = false, notification = "trashItemsRestoreSuccess" });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return JsonConvert.SerializeObject(new { error = true, notification = "trashItemsRestoreError" });
            }
        }
        [HttpPost]
        [Authorize(Roles = "Supervisor,ModifyTrash")]
        public string DeleteForever(string value)
        {
            try
            {
                var entities = JsonConvert.DeserializeObject<List<Entity>>(value);
                foreach (var entity in entities)
                {
                    var entityModified = EntityService.findEntity(entity);       
                    db.Entry(entityModified).State = EntityState.Deleted;
                }
                db.SaveChanges();
                return JsonConvert.SerializeObject(new { error = false, notification = "trashItemsDeletedForeverSuccess" });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return JsonConvert.SerializeObject(new { error = true, notification = "trashItemsDeletedForeverError" });
            }
        }
    }
}