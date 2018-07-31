using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.DAL.Interfaces;
using Ferretto.Common.Models;

namespace Ferretto.Common.DAL.EF
{
  public class ItemsRepository : Repository<Item>, IItemsRepository
  {
    public ItemsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    { }

    public IEnumerable<Item> List(int skip, int take)
    {
      return unitOfWork.Context.Items
        .Skip(skip)
        .Take(take)
        .AsEnumerable();
    }
  }
}
