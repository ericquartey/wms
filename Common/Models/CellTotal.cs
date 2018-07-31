namespace Ferretto.Common.Models
{
  // Totali Celle
  public partial class CellTotal
  {
    public int Id { get; set; }
    public int AisleId { get; set; }
    public int CellTypeId { get; set; }
    public int CellStatusId { get; set; }
    public int CellsNumber { get; set; }

    public Aisle Aisle { get; set; }
    public CellType CellType { get; set; }
    public CellStatus CellStatus { get; set; }
  }
}
