using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMeasureUnitProvider :
        IReadAllAsyncProvider<MeasureUnit, string>,
        IReadSingleAsyncProvider<MeasureUnit, string>
    {
    }
}
