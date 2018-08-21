using Ferretto.Common.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.Common.Modules.DAL.EF
{
  public class Repository<T> : IDisposable, IRepositoryInt<T> where T : class
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
      return this.unitOfWork.Context.Set<T>().Find(id);
    }

    public IEnumerable<T> List()
    {
      return this.unitOfWork.Context.Set<T>().AsEnumerable();
    }

    public T Insert(T entity)
    {
      this.unitOfWork.Context.Set<T>().Add(entity);

      return entity;
    }

    public void Update(T entity)
    {
      this.unitOfWork.Context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(T entity)
    {
      this.unitOfWork.Context.Set<T>().Remove(entity);
    }

    public virtual void Dispose()
    {
      if (this.unitOfWork is IDisposable disposable)
      {
        disposable.Dispose();
      }
    }
  }
}
