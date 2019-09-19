using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataModels.Elevator
{
    public sealed class MovementProfile : DataModel
    {
        #region Properties

        public IEnumerable<MovementParameters> Steps { get; set; }

        #endregion
    }
}
