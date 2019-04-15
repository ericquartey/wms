using System.Threading.Tasks;

namespace Ferretto.VW.Utils.Interfaces
{
    public interface IViewModel
    {
        #region Methods

        void ExitFromViewMethod();

        Task OnEnterViewAsync();

        void UnSubscribeMethodFromEvent();

        #endregion
    }
}
