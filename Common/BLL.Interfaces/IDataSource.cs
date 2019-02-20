using Ferretto.Common.BLL.Interfaces.Base;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IDataSource<TModel, TKey>
        where TModel : IModel<TKey>
    {
    }
}
