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

    public virtual void OnAppear()
    {
    }

    public void Appear()
    {
    }

    public void Disappear()
    {
    }
  }
}
