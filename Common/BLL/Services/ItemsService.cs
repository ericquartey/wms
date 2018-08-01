using System.Collections.Generic;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.DAL.Interfaces;
using Ferretto.Common.Models;

namespace Ferretto.Common.BLL.Services
{
  public class ItemsService : IItemsService
  {
    private readonly IItemsRepository repository;

    public ItemsService(IItemsRepository repository)
    {
      this.repository = repository;
    }

    public IEnumerable<Item> GetItems()
    {
      return this.repository.List();
    }
  }
}
