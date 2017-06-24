using Reta.Controllers.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Reta.Models
{
    public class EmailTemplate : IEntity
    {      
        [Key]
        public string Id { get; set; }

        [Display(Name = "Nom du template")]
        public string Name { get; set; }

        [Display(Name = "Type d'email")]
        public string Type { get; set; }

        [Display(Name = "Objet de l'email")]
        public string Subject { get; set; }

        [Display(Name = "Corps d'email")]
        [DataType(DataType.MultilineText)]
        public string Body { get; set; }

        public string BodyUrl { get; set; }

        public string Lang { get; set; }

        public string DynamicField { get; set; } // to store Json object
        public string SpaceId { get; set; }
        [ForeignKey("SpaceId")]
        public Space Space { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string Metadata { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string DeletedBy { get; set; }

        public EmailTemplate()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}