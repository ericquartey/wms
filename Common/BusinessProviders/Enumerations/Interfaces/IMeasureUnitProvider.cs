using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IMeasureUnitProvider : IReadAllAsyncProvider<EnumerationString>,
        IReadSingleAsyncProvider<EnumerationString, string>
    {
    }
}
