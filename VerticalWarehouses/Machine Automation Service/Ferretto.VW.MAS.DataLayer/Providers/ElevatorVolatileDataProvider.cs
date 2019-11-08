using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    public class ElevatorVolatileDataProvider : IElevatorVolatileDataProvider
    {
        #region Properties

        public double HorizontalPosition { get; set; }

        public double VerticalPosition { get; set; }

        #endregion
    }
}
