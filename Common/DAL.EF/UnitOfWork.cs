using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.DAL.Interfaces;

namespace Ferretto.Common.DAL.EF
{
  public class UnitOfWork : IUnitOfWork, IDisposable
  {
    public DbContext Context { get; private set; }

    public UnitOfWork(DbContext context)
    {
      if (context == null)
      {
        throw new ArgumentNullException(nameof(context));
      }

      Context = context;
    }

    public void Commit()
    {
      Context.SaveChanges();
    }

    void IDisposable.Dispose()
    {
      Context.Dispose();
    }

    internal void Dispose()
    {
      throw new NotImplementedException();
    }
  }
}
