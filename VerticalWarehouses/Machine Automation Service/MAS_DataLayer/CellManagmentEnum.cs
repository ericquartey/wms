using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS_DataLayer
{
    public enum Side : long
    {
        FrontEven,
        BackOdd
    }

    public enum Status : long
    {
        Free,
        Disabled,
        Occupied,
        Unusable
    }
}
