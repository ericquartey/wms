using Ferretto.Common.Controls.Interfaces;
using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
  public class BaseViewModel : BindableBase, INavigableViewModel
  {
    public string StateId { get; set; }
    public string Token { get; set; }

    protected BaseViewModel()
    {

    }

    public virtual void OnAppear()
    {

    }
  }
}
