using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Unità di Misura
  public partial class MeasureUnit
  {
    public int Id { get; set; }
    public string Description { get; set; }

    public List<Item> Items { get; set; }
  }
}
