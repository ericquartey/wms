using System;
using System.Linq;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IDataSource<TModel> where TModel : IBusinessObject
    {
        #region Properties

        Func<IQueryable<TModel>> GetData { get; }
        Func<int> GetDataCount { get; }
        string Name { get; }

        #endregion Properties
    }
}
