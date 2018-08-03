using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Baia
  public sealed class Bay
  {
    public int Id { get; set; }
    public string BayTypeId { get; set; }
    public int? LoadingUnitsBufferSize { get; set; }
    public string Description { get; set; }

    public BayType BayType { get; set; }

    public List<Mission> SourceMissions { get; set; }
    public List<Mission> DestinationMissions { get; set; }
  }
}
