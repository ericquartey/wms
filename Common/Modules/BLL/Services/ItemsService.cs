using System.Collections.Generic;
using AutoMapper;
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
      return Mapper.Map<IEnumerable<IItem>>(this.repository.List());
    }

    public IItem GetById(int id)
    {
      return Mapper.Map<IItem>(this.repository.GetById(id));
    }

    public IItem Create(IItem item)
    {
      return Mapper.Map<IItem>(this.repository.Insert(Mapper.Map<Common.Models.Item>(item)));
    }
  }
}
