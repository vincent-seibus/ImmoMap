using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reta.Models.Helpers
{
    public class Entity : IEntity
    {
        public string CreatedBy { get; set; }

        public DateTime? CreatedOn { get; set; }

        public string DeletedBy { get; set; }

        public DateTime? DeletedOn { get; set; }

        public string Id { get; set; }

        public string Metadata { get; set; }

        public string SpaceId { get; set; }

        public string EntityType { get; set; }

        public string Name { get; set; }
    }
}