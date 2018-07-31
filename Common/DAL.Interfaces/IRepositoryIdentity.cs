using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.Common.DAL.Interfaces
{
  public interface IRepositoryIdentity<TEntity, in TId>
  {
    TEntity GetById(TId id);

    IEnumerable<TEntity> List();

    void Insert(TEntity entity);

    void Update(TEntity entity);

    void Delete(TEntity entity);
  }
}
