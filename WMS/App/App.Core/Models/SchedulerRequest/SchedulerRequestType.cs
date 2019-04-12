using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.WMS.App.Core.Models
{
    public enum SchedulerRequestType
    {
        Item = 'I',

        LoadingUnit = 'U',

        ItemList = 'L',

        ItemListRow = 'R',
    }
}
