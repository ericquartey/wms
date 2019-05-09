using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.WMS.Data.Core.Models
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
