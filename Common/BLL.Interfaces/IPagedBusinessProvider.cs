using Ferretto.Common.BLL.Interfaces.Base;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IPagedBusinessProvider<TModel, TKey> :
        IReadAllPagedAsyncProvider<TModel, TKey>,
        IGetUniqueValuesAsyncProvider
        where TModel : IModel<TKey>
    {
    }
}
