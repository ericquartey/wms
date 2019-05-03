using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.WMS.Data.Core.Models
{
    public class BayScheduler : Model<int>
    {
        #region Properties

        public int? LoadingUnitsBufferSize { get; set; }

        public int LoadingUnitsBufferUsage { get; internal set; }

        #endregion

        // TODO: should LoadingUnitsBufferSize this be a non-nullable?
    }
}
