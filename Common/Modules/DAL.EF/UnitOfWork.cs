using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.DAL.Interfaces;

namespace Ferretto.Common.Modules.DAL.EF
{
  public sealed class UnitOfWork : IUnitOfWork, IDisposable
  {
    public DatabaseContext Context { get; private set; }

    public UnitOfWork(DatabaseContext context)
    {
      this.Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public void Commit()
    {
      this.Context.SaveChanges();
    }

    public void Dispose()
    {
      this.Context?.Dispose();
    }
  }
}
