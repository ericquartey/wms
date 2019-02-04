namespace Ferretto.Common.DataModels
{
    // Configurazione Cella - Posizione in Cella - Tipo Udc
    public sealed class CellConfigurationCellPositionLoadingUnitType
    {
        #region Properties

        public CellConfiguration CellConfiguration { get; set; }

        public int CellConfigurationId { get; set; }

        public CellPosition CellPosition { get; set; }

        public int CellPositionId { get; set; }

        public LoadingUnitType LoadingUnitType { get; set; }

        public int LoadingUnitTypeId { get; set; }

        public int Priority { get; set; }

        #endregion
    }
}
