using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.VW.Installer.ViewModels;

#nullable enable

namespace Ferretto.VW.Installer.Services
{
    public interface INavigationService : INotifyPropertyChanged
    {
        #region Properties

        IViewModel? ActiveViewModel { get; }

        #endregion

        #region Methods

        void NavigateBack();

        Task NavigateToAsync(IViewModel viewModel);

        #endregion
    }
}
