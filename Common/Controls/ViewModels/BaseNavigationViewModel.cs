using Ferretto.Common.Controls.Interfaces;
using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
  public class BaseNavigationViewModel : BindableBase, INavigableViewModel
  {
    public string StateId { get; set; }
    public string Token { get; set; }

    protected BaseNavigationViewModel()
    {

    }

    protected virtual void OnAppear()
    {
      // Nothing to do here.
      // Derived classes can implement custom logic overriding this method.
    }

    public void Appear()
    {
      this.OnAppear();
    }

    public void Disappear()
    {
    }
  }
}
