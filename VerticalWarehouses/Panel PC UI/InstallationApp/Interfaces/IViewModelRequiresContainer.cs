using Unity;

namespace Ferretto.VW.App.Installation.Interfaces
{
    public interface IViewModelRequiresContainer
    {
        #region Methods

        void InitializeViewModel(IUnityContainer container);

        #endregion
    }
}
