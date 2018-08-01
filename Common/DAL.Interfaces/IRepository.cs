using System.Collections.Generic;

namespace Ferretto.Common.DAL.Interfaces
{
  public interface IRepository<TEntity, in TId> where TEntity : class
  {
    TEntity GetById(TId id);

    IEnumerable<TEntity> List();

    void Insert(TEntity entity);

    void Update(TEntity entity);

    void Delete(TEntity entity);
  }
}
