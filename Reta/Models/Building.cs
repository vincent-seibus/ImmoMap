using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Reta.Models
{
    public class Building : IEntity
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
        public virtual BuildingConfiguration ConfigurationParsed {
            get
            {
                if (!string.IsNullOrEmpty(Configuration))
                    return JsonConvert.DeserializeObject<BuildingConfiguration>(Configuration);
                else
                    return null;
            }
            set
            {
                Configuration = JsonConvert.SerializeObject(value);
            }
        }

        public Building()
        {
            Id = Guid.NewGuid().ToString();
        }
    }

    public class BuildingConfiguration
    {
        public string description { get; set; }
        public string fullAddress { get; set; }
        public string streetNumber { get; set; }
        public string streetName { get; set; }
        public string postcode { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public double latitude { get; set; }
        public double? longitude { get; set; }
        public double? altitude { get; set; }
        public List<Coordinate> coordinates { get; set; }
        public string comment { get; set; }
        public double? lastPrice { get; set; }
        public DateTime? lastPriceDate { get; set; }
        public double? surfaceTotalInM2 { get; set; }
        public List<SurfaceDetail> surfaces { get; set; }
        public List<Transaction> transactionHistory { get; set; }
        public Company owner { get; set; }
        public Company tenant { get; set; }
        public double? currentRent { get; set; }
        public DateTime? leaseStartedOn { get; set; }
        public DateTime? leaseFinishedOn { get; set; }
    }

    public class SurfaceDetail
    {
        public string description { get; set; }
        public string surfaceInM2 { get; set; }
    }

    public class Coordinate
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }

    public class Transaction : IEntity
    {
        public double? amount { get; set; }
        public double? currency { get; set; }
        public Company buyer { get; set; }
        public Company seller { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Metadata { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string SpaceId { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}