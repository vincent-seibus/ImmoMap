using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Reta.Models
{
    public class Space : IEntity
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Configuration { get; set; } // will be stored as Json string
        public string Metadata { get; set; } // will be stored as Json string
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string SpaceId { get; set; }
        public Space()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}