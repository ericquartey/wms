using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.Utils.Expressions;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IPagedBusinessProvider<TModel> :
        IReadAllPagedAsyncProvider<TModel>,
        IGetUniqueValuesAsyncProvider
        where TModel : IBusinessObject
    {
    }
}
