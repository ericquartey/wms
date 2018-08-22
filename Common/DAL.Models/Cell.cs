using System.Collections.Generic;

namespace Ferretto.Common.DAL.Models
{
  // Cella
  public sealed class Cell
  {
    public int Id { get; set; }
    public int AisleId { get; set; }
    public int? Floor { get; set; }
    public int? Column { get; set; }
    public Side Side { get; set; }
    public int? CellNumber { get; set; }
    public int? XCoordinate { get; set; }
    public int? YCoordinate { get; set; }
    public int? ZCoordinate { get; set; }
    public int Priority { get; set; }
    public int? CellTypeId { get; set; }
    public string ClassId { get; set; }
    public int CellStatusId { get; set; }

    public AbcClass AbcClass { get; set; }
    public Aisle Aisle { get; set; }
    public CellStatus CellStatus { get; set; }
    public CellType CellType { get; set; }

    public IEnumerable<LoadingUnit> LoadingUnits { get; set; }
    public IEnumerable<CellsGroup> FirstCellsGroups { get; set; }
    public IEnumerable<CellsGroup> LastCellsGroups { get; set; }
    public IEnumerable<Mission> SourceMissions { get; set; }
    public IEnumerable<Mission> DestinationMissions { get; set; }
  }
}
