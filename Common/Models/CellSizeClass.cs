using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Classe Dimensione Cella
    public partial class CellSizeClass
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }

        public List<CellType> CellTypes { get; set; }
    }
}
