namespace Ferretto.Common.DataModels
{
    // Tipo Cella-Corridoio
    public sealed class CellTypeAisle : IDataModel
    {
        #region Properties

        public Aisle Aisle { get; set; }

        public int AisleId { get; set; }

        public CellType CellType { get; set; }

        public int CellTypeId { get; set; }

        public int CellTypeTotal { get; set; }

        public int Id { get; set; }

        public decimal Ratio { get; set; }

        #endregion
    }
}
