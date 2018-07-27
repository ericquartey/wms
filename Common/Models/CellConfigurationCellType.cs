namespace Ferretto.Common.Models
{
    // Configurazione Cella - Tipo Cella
    public partial class CellConfigurationCellType
    {
        public int CellConfigurationId { get; set; }
        public int CellTypeId { get; set; }
        public int Priority { get; set; }

        public CellConfiguration CellConfiguration { get; set; }
        public CellType CellType { get; set; }
    }
}
