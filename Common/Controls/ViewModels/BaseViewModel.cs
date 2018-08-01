using Ferretto.Common.Controls.Interfaces;
using Prism.Mvvm;

namespace Ferretto.Common.Controls
{

  public partial class BaseViewModel : BindableBase, INavigableViewModel
  {  
    public string StateId { get; set; }
    public string Token { get; set; }

    public virtual void OnAppear()
    {
    }
  }
}
