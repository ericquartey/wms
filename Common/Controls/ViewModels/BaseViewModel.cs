using Ferretto.Common.Controls.Interfaces;
using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
  public abstract class BaseViewModel : BindableBase, INavigableViewModel
  {
    public string StateId { get; set; }
    public string Token { get; set; }

    public abstract void OnAppear();
  }
}
