using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IMissionProvider : IPagedBusinessProvider<Mission>,
        IReadSingleAsyncProvider<Mission, int>
    {
    }
}
