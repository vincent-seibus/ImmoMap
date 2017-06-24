using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reta.Models
{
    public class Company : IEntity
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Configuration { get; set; }
        public string Metadata { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string SpaceId { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }

        [NotMapped]
        public virtual CompanyConfiguration ConfigurationParsed
        {
            get
            {
                if (!string.IsNullOrEmpty(Configuration))
                    return JsonConvert.DeserializeObject<CompanyConfiguration>(Configuration);
                else
                    return null;
            }
            set
            {
                Configuration = JsonConvert.SerializeObject(value);
            }
        }

        public Company()
        {
            Id = Guid.NewGuid().ToString();
        }
    }

    public class CompanyConfiguration
    {
        public string idNumber { get; set; }
        public string name { get; set; }
        public string turnover { get; set; }
    }
}