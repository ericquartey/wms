using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IMaterialStatusProvider : IReadAllAsyncProvider<Enumeration>,
        IReadSingleAsyncProvider<Enumeration, int>
    {
    }
}
