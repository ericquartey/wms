using Unity;

namespace Ferretto.VW.InstallationApp
{
    public interface IViewModelRequiresContainer
    {
        #region Methods

        void InitializeViewModel(IUnityContainer container);

        #endregion
    }
}
