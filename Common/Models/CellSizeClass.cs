using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Classe Dimensione Cella
    public sealed class CellSizeClass
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }

        public IEnumerable<CellType> CellTypes { get; set; }
    }
}
