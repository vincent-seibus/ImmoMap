using Reta.Models;
using Reta.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reta.Controllers.Helpers
{
    public static class EntityService
    {
      
        public static IEntity OnCreateEntity(IEntity entity, string UserId)
        {
            entity.CreatedBy = UserId;
            entity.CreatedOn = DateTime.UtcNow;
            SpaceService spaceService = new SpaceService();
            entity.SpaceId = spaceService.GetSpaceIdFromUser(UserId);
            return entity;
        }

        public static IEntity OnDeleteEntity(IEntity entity, string UserId)
        {
            entity.DeletedBy = UserId;
            entity.DeletedOn = DateTime.UtcNow;           
            return entity;
        }

        public static IEntity OnRestoreEntity(IEntity entity)
        {
            entity.DeletedBy = null;
            entity.DeletedOn = null;           
            return entity;
        }

        public static Entity CastIEntityToEntity(IEntity entity)
        {
            Entity obj = new Entity();
            var type = entity.GetType();
            obj.Id = entity.Id;
            obj.Name = entity.Name;
            obj.SpaceId = entity.SpaceId;
            obj.DeletedOn = entity.DeletedOn;
            obj.EntityType = type.Name.ToString();
            return obj;
        }

        public static IEntity findEntity(Entity entity)
        {
             MySqlIdentityDbContext db = new MySqlIdentityDbContext();
             var ent = db.Entry(entity).Entity;
             return ent;
        }
    }
}