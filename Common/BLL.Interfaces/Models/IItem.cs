using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.Common.BLL.Interfaces.Models
{
  public interface IItem : IModel<int>
  {
    string Code { get; set; }
    string Description { get; set; }
    string ClassId { get; set; }
    int? MeasureUnitId { get; set; }
    int? Width { get; set; }
    int? Length { get; set; }
    int? Height { get; set; }
  }
}
