using System.Collections.Generic;
using Ferretto.Common.Models;

namespace Ferretto.Common.DAL.Interfaces
{
  public interface IItemsRepository : IRepositoryInt<Item>
  {
    IEnumerable<Item> List(int skip, int take);
  }
}
