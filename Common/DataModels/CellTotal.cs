namespace Ferretto.Common.DataModels
{
    // Totali Celle
    public sealed class CellTotal : IDataModel
    {
        #region Properties

        public Aisle Aisle { get; set; }

        public int AisleId { get; set; }

        public int CellsNumber { get; set; }

        public CellStatus CellStatus { get; set; }

        public int CellStatusId { get; set; }

        public CellType CellType { get; set; }

        public int CellTypeId { get; set; }

        public int Id { get; set; }

        #endregion Properties
    }
}
