using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTHorizontalEngineViewModel : BindableBase, IViewModel, ILSMTHorizontalEngineViewModel
    {
        #region Fields

        private IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public LSMTHorizontalEngineViewModel(IEventAggregator eventAggregator)
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
