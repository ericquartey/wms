using System.Threading.Tasks;

#nullable enable

namespace Ferretto.VW.Installer.ViewModels
{
    public interface IViewModel
    {
        #region Methods

        Task OnAppearAsync();

        Task OnDisappearAsync();

        #endregion
    }
}
