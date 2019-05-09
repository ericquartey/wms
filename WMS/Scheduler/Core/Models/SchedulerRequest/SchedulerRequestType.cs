using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public enum SchedulerRequestType
    {
        NotSpecified = 0,

        Item = 'I',

        LoadingUnit = 'U',

        ItemList = 'L',

        ItemListRow = 'R',
    }
}
