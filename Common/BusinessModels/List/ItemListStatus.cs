using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.Common.BusinessModels
{
    public enum ItemListStatus
    {
        Waiting = 'W',
        Executing = 'E',
        Completed = 'C',
        Incomplete = 'I',
        Suspended = 'S'
    }
}
