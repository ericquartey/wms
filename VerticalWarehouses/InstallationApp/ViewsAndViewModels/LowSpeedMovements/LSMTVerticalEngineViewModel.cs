using Ferretto.VW.ActionBlocks;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTVerticalEngineViewModel : BindableBase, IViewModel, ILSMTVerticalEngineViewModel
    {
        #region Fields

        public IUnityContainer Container;

        private IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public LSMTVerticalEngineViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.Container = _container;
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
