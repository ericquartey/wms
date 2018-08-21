using System.Collections.Generic;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.DAL.Interfaces;

namespace Ferretto.Common.Modules.BLL.Services
{
  public class ItemsService : IItemsService
  {
    private readonly IItemsRepository repository;

    public ItemsService(IItemsRepository repository)
    {
      this.repository = repository;
    }

    public IEnumerable<IItem> GetAll()
    {
      return this.repository.List();
    }

    public IItem GetById(int id)
    {
    }

    public IItem Create(IItem item)
    {
    }
  }
}
