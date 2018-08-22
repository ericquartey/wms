namespace Ferretto.Common.DAL.Models
{
  // Tipo Udc - Corridoio
  public sealed class LoadingUnitTypeAisle
  {
    public int AisleId { get; set; }
    public int LoadingUnitTypeId { get; set; }

    public Aisle Aisle { get; set; }
    public LoadingUnitType LoadingUnit { get; set; }
  }
}
