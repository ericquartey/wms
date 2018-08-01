using Ferretto.Common.Models;
using System.Collections.Generic;

namespace Ferretto.Common.BLL.Interfaces
{
  public interface IItemsService
  {
    IEnumerable<Item> GetItems();
  }
}
