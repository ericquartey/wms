using System.Collections.Generic;

namespace Ferretto.Common.DAL.Models
{
  // Tipo Baia
  public sealed class BayType
  {
    public string Id { get; set; }
    public string Description { get; set; }

    public List<Bay> Bays { get; set; }
  }
}
