using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSShutterViewModel : BindableBase, IViewModel, ISSShutterViewModel
    {
        #region Fields

        private IEventAggregator eventAggregator;

        private bool gateSensorA;

        private bool gateSensorB = true;

        #endregion

        #region Constructors

        public SSShutterViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public bool GateSensorA { get => this.gateSensorA; set => this.SetProperty(ref this.gateSensorA, value); }

        public bool GateSensorB { get => this.gateSensorB; set => this.SetProperty(ref this.gateSensorB, value); }

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
