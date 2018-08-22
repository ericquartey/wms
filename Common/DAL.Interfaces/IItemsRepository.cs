using System.Collections.Generic;

namespace Ferretto.Common.DAL.Interfaces
{
  public interface IItemsRepository : IRepositoryInt<Models.Item>
  {
    IEnumerable<Models.Item> List(int skip, int take);
  }
}
