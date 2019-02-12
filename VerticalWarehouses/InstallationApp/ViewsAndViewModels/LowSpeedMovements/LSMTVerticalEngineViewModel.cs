using Ferretto.VW.ActionBlocks;
using Microsoft.Practices.Unity;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LSMTVerticalEngineViewModel : BindableBase, IViewModel, ILSMTVerticalEngineViewModel
    {
        #region Fields

        public IUnityContainer Container;

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            throw new System.NotImplementedException();
        }

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.Container = _container;
        }

        public void SubscribeMethodToEvent()
        {
            throw new System.NotImplementedException();
        }

        public void UnSubscribeMethodFromEvent()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
