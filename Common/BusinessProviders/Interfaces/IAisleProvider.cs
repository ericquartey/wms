using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IAisleProvider :
        IReadAllAsyncProvider<Aisle>,
        IReadSingleAsyncProvider<Aisle, int>
    {
    }
}
