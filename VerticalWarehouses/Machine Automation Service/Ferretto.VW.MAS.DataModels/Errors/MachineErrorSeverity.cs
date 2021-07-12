using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataModels
{
    public enum MachineErrorSeverity
    {
        None = -1,

        Low = 0,

        Normal = 1,

        High = 2,

        NeedsHoming = 3,
    }
}
