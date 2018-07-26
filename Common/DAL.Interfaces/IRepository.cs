using System.Collections.Generic;

namespace Ferretto.Common.DAL.Interfaces
{
  public interface IRepository<T>
  {
    T GetById(int id);

    IEnumerable<T> List();

    void Insert(T entity);

    void Update(T entity);

    void Delete(T entity);

    void SaveChanges();
  }
}
