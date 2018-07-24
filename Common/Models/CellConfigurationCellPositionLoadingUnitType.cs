namespace Ferretto.Common.Models
{
    // Configurazione Cella - Posizione in Cella - Tipo Udc
    public partial class CellConfigurationCellPositionLoadingUnitType
    {
        public int CellPositionId { get; set; }
        public int CellConfigurationId { get; set; }
        public int LoadingUnitTypeId { get; set; }
        public int Priority { get; set; }

        public CellPosition CellPosition { get; set; }
        public CellConfiguration CellConfiguration { get; set; }
        public LoadingUnitType LoadingUnitType { get; set; }
    }
}
