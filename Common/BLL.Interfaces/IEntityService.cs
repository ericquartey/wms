using System.Collections.Generic;

namespace Ferretto.Common.BLL.Interfaces
{
  public interface IEntityService<TModel, in TId> where TModel : IModel<TId>
  {
    IEnumerable<TModel> GetAll();
    TModel GetById(TId id);
    TModel Create(TModel item);
  }
}
