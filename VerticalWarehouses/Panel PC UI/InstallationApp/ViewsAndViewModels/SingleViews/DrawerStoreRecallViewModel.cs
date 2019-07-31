using System.Threading.Tasks;
using Ferretto.VW.App.Installation.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews
{
    public class DrawerStoreRecallViewModel : BindableBase, IDrawerStoreRecallViewModel
    {
        #region Properties

        public BindableBase NavigationViewModel { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            //TODO Add your implementation code here
        }

        public Task OnEnterViewAsync()
        {
            //TODO Add your implementation code here
            return Task.CompletedTask;
        }

        public void UnSubscribeMethodFromEvent()
        {
            //TODO Add your implementation code here
        }

        #endregion
    }
}
