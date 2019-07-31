using System.Threading.Tasks;
using Ferretto.VW.Utils.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Controls
{
    public class BaseViewModel : BindableBase, IViewModel
    {
        #region Properties

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public virtual void ExitFromViewMethod()
        {
            // do nothing.
            // derived classes can customize the behaviour of this method.
        }

        public virtual Task OnEnterViewAsync()
        {
            // do nothing.
            // derived classes can customize the behaviour of this method.
            return Task.CompletedTask;
        }

        public virtual void UnSubscribeMethodFromEvent()
        {
            // do nothing.
            // derived classes can customize the behaviour of this method.
        }

        #endregion
    }
}
