using System.Diagnostics;
using System.Windows.Input;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTShutterEngineViewModel : BindableBase, IViewModel, ILSMTShutterEngineViewModel
    {
        #region Fields

        private IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public LSMTShutterEngineViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
