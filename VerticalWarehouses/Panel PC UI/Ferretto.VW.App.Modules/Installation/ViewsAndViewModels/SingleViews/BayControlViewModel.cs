using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews
{
    internal class BayControlViewModel : BindableBase
    {
        #region Fields

        private ICommand exitFromViewCommand;

        #endregion

        #region Properties

        public ICommand ExitFromViewCommand => this.exitFromViewCommand ?? (this.exitFromViewCommand = new DelegateCommand(this.ExitFromViewMethod));

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            this.UnSubscribeMethodFromEvent();
        }

        public Task OnEnterViewAsync()
        {
            return Task.CompletedTask;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
