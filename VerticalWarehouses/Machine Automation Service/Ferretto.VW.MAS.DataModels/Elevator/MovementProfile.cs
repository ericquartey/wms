using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class MovementProfile : DataModel
    {
        #region Properties

        public IEnumerable<MovementParameters> Steps { get; set; }

        #endregion
    }
}
