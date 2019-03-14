using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.BLL.Interfaces.Providers
{
    public interface IPagedBusinessProvider<TModel, TKey> :
        IReadAllPagedAsyncProvider<TModel, TKey>,
        IGetUniqueValuesAsyncProvider
        where TModel : IModel<TKey>
    {
    }
}
