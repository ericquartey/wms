using Ferretto.Common.DAL.Interfaces;
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
    protected readonly UnitOfWork unitOfWork;

    public Repository(IUnitOfWork unitOfWork)
    {
      if (unitOfWork is UnitOfWork unitOfWorkEf)
      {
        this.unitOfWork = unitOfWorkEf;
      }
      else
      {
        throw new ArgumentException("Injected Unit of Work for Entity Framework repository is not valid.", nameof(unitOfWork));
      }
    }

    public T GetById(int id)
    {
      return unitOfWork.Context.Set<T>().Find(id);
    }

    public IEnumerable<T> List()
    {
      return unitOfWork.Context.Set<T>().AsEnumerable();
    }

    public void Insert(T entity)
    {
      unitOfWork.Context.Set<T>().Add(entity);
    }

    public void Update(T entity)
    {
      unitOfWork.Context.Entry(entity).State = EntityState.Modified;
    }
  
    public void Delete(T entity)
    {
      unitOfWork.Context.Set<T>().Remove(entity);
    }

    void IDisposable.Dispose()
    {
      if (unitOfWork is IDisposable)
      {
        unitOfWork.Dispose();
      }
    }
  }
}
