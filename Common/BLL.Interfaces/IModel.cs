using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.Common.BLL.Interfaces
{
  public interface IModel<TId>
  {
    TId Id { get; set; }
  }
}
