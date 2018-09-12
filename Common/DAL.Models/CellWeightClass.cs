using System.Collections.Generic;

namespace Ferretto.Common.DAL.Models
{
    // Classe Peso Cella
    public sealed class CellWeightClass
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int MinWeight { get; set; }
        public int MaxWeight { get; set; }

        public IEnumerable<CellType> CellTypes { get; set; }
    }
}
