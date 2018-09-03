using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.Common.BLL.Interfaces.Models
{
  public interface IFilter : IModel<int>
  {
    string Name { get;set; }

    int Count { get; set; }
  }
}
