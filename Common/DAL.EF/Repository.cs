using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.Common.DAL.EF
{
  public class Repository<T> : IDisposable, Interfaces.IRepository<T> where T : class
  {
    protected readonly DbContext dbContext;

    public Repository(DbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    public T GetById(int id)
    {
      return dbContext.Set<T>().Find(id);
    }

    public IEnumerable<T> List()
    {
      return dbContext.Set<T>().AsEnumerable();
    }

    public void Insert(T entity)
    {
      dbContext.Set<T>().Add(entity);
    }

    public void Update(T entity)
    {
      dbContext.Entry(entity).State = EntityState.Modified;
    }
  
    public void Delete(T entity)
    {
      dbContext.Set<T>().Remove(entity);
    }

    public void SaveChanges()
    {
      dbContext.SaveChanges();
    }

    void IDisposable.Dispose()
    {
      if (dbContext != null)
      {
        dbContext.Dispose();
      }
    }
  }
}
