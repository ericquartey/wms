using System.Threading.Tasks;
using Ferretto.VW.App.Installation.Interfaces;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.ShuttersControl
{
    public class Shutter2ControlViewModel : BindableBase, IShutter2ControlViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public Shutter2ControlViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
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
