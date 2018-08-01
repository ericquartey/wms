using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.Common.BLL.Interfaces
{
  public interface IEventService
  {
    void Invoke<T>(T eventArgs) where T : IEventArgs;
  }
}
