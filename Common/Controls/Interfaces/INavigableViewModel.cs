using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.Common.Controls.Interfaces
{
  public interface INavigableViewModel
  {
    string Token { get; set; }
    string StateId { get; set; }
    void OnAppear();
    void Appear();
    void Disappear();
  }
}
