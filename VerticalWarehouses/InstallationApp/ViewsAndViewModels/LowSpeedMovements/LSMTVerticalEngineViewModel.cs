using Ferretto.VW.ActionBlocks;
using Microsoft.Practices.Unity;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTVerticalEngineViewModel : BindableBase, IViewModel, ILSMTVerticalEngineViewModel
    {
        public IUnityContainer Container;
        public PositioningDrawer PositioningDrawer;

        #region Methods

        public void ExitFromViewMethod()
        {
            throw new System.NotImplementedException();
        }

        public void SubscribeMethodToEvent()
        {
            throw new System.NotImplementedException();
        }

        public void UnSubscribeMethodFromEvent()
        {
            throw new System.NotImplementedException();
        }

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.Container = _container;
            this.PositioningDrawer = (PositioningDrawer)this.Container.Resolve<IPositioningDrawer>();
        }

        #endregion Methods
    }
}
