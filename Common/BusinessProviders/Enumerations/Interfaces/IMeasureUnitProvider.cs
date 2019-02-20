using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IMeasureUnitProvider : IReadAllAsyncProvider<EnumerationString, string>,
        IReadSingleAsyncProvider<EnumerationString, string>
    {
    }
}
