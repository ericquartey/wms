namespace Ferretto.Common.Models
{
  // Gruppi celle
  public sealed class CellsGroup
  {
    public int Id { get; set; }
    public int AisleId { get; set; }
    public int? Priority { get; set; }
    public int FirstCellId { get; set; }
    public int LastCellId { get; set; }
    public int GroupHeight { get; set; }

    public Aisle Aisle { get; set; }
    public Cell FirstCell { get; set; }
    public Cell LastCell { get; set; }
  }
}
