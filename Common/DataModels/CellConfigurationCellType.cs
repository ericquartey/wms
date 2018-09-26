namespace Ferretto.Common.DataModels
{
    // Configurazione Cella - Tipo Cella
    public sealed class CellConfigurationCellType
    {
        public int CellConfigurationId { get; set; }
        public int CellTypeId { get; set; }
        public int Priority { get; set; }

        public CellConfiguration CellConfiguration { get; set; }
        public CellType CellType { get; set; }
    }
}
