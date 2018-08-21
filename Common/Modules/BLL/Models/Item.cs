using System;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Models;

namespace Ferretto.Common.Modules.BLL.Models
{
  public sealed class Item : IItem
  {
    public int Id { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public AbcClass AbcClass { get; set; }
    public MeasureUnit MeasureUnit { get; set; }

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
