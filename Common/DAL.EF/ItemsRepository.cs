using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.Models;

namespace Ferretto.Common.DAL.EF
{
  public class ItemsRepository : Repository<Item>, Interfaces.IItemsRepository
  {
    public ItemsRepository(DbContext dbContext) : base(dbContext)
    { }

    public IEnumerable<Item> List(int skip, int take)
    {
      return dbContext.Items
        .Skip(skip)
        .Take(take)
        .AsEnumerable();
    }
  }
}
