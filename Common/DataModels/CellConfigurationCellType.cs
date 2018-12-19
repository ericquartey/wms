namespace Ferretto.Common.DataModels
{
    // Configurazione Cella - Tipo Cella
    public sealed class CellConfigurationCellType
    {
        #region Properties

        public CellConfiguration CellConfiguration { get; set; }

        public int CellConfigurationId { get; set; }

        public CellType CellType { get; set; }

        public int CellTypeId { get; set; }

        public int Priority { get; set; }

        #endregion Properties
    }
}
