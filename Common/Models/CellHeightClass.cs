using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Classe Altezza Cella
    public partial class CellHeightClass
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int MinHeight { get; set; }
        public int MaxHeight { get; set; }

        public List<CellType> CellTypes { get; set; }
    }
}
