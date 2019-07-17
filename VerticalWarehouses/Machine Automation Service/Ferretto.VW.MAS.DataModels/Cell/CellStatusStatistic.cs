using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class CellStatusStatistic
    {
        #region Properties

        public double RatioBackCells { get; set; }

        public double RatioFrontCells { get; set; }

        public CellStatus Status { get; set; }

        public int TotalBackCells { get; set; }

        public int TotalFrontCells { get; set; }

        #endregion
    }
}
