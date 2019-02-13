using System;

namespace Ferretto.VW.MAS_DataLayer
{
    public class Cell
    {
        public int CellId { get; set; }

        public int Coord { get; set; }

        public int Priority { get; set; }

        public Side Side { get; set; }

        public Status Status { get; set; }
    }
}
