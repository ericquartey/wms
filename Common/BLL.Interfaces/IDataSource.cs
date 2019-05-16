using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IDataSource<TModel, TKey>
        where TModel : IModel<TKey>
    {
        #region Properties

        string Key { get; }

        string Name { get; }

        #endregion
    }
}
