using System.Collections.Generic;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.DAL.Interfaces;
using Ferretto.Common.Models;

namespace Ferretto.Common.BLL.Services
{
  public class ItemsService : IItemsService
  {
    private readonly IRepository<Item> repository;

    public ItemsService(IRepository<Item> repository)
    {
      this.repository = repository;
    }

    public IEnumerable<Item> GetItems()
    {
      return repository.List();
    }
  }
}
