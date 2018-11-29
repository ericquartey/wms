using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.Common.BusinessModels
{
    public enum ItemListStatus
    {
        Waiting = 1,
        Executing = 2,
        Completed = 3,
        Incomplete = 4,
        Suspended = 5
    }
}
