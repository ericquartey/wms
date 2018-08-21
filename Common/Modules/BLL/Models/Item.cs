using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Modules.BLL.Models
{
  public sealed class Item : IModel<int>
  {
    public int Id { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public string ClassId { get; set; }
    public int? MeasureUnitId { get; set; }

    private int? width;
    public int? Width
    {
      get { return this.width; }
      set
      {
        if (value.HasValue && value <= 0)
        {
          throw new ArgumentOutOfRangeException(nameof(value), "Parameter must be strictly positive.");
        }

        this.width = value;
      }
    }

    private int? length;
    public int? Length
    {
      get { return this.length; }
      set
      {
        if (value.HasValue && value <= 0)
        {
          throw new ArgumentOutOfRangeException(nameof(value), "Parameter must be strictly positive.");
        }

        this.length = value;
      }
    }

    private int? height;
    public int? Height
    {
      get { return this.height; }
      set
      {
        if (value.HasValue && value <= 0)
        {
          throw new ArgumentOutOfRangeException(nameof(value), "Parameter must be strictly positive.");
        }

        this.height = value;
      }
    }
  }
}
