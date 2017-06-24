using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reta.Models
{
    public interface IEntity
    {
        string Id { get; set; }
        string Name { get; set; }
        string Metadata { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string SpaceId { get; set; }
        string DeletedBy { get; set; }
        DateTime? DeletedOn { get; set; }
    }
}
