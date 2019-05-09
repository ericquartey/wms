using System.Threading.Tasks;
using Prism.Mvvm;

namespace Ferretto.VW.Utils.Interfaces
{
    public interface IViewModel
    {
        #region Properties

        BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        void ExitFromViewMethod();

        Task OnEnterViewAsync();

        void UnSubscribeMethodFromEvent();

        #endregion
    }
}
