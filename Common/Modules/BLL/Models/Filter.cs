using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.Modules.BLL.Models
{
  public class Filter : IFilter
  {
    public int Id { get; set; }

    public string Name { get; set; }

    public int Count { get; set; }
  }
}
