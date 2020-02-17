using System;
using System.Threading.Tasks;
using Prism.Regions;

namespace Ferretto.VW.App.Services
{
    public interface INavigableViewModel : IDisposable
    {
        #region Properties

        bool IsVisible { get; }

        #endregion

        #region Methods

        bool CanClose();

        void Disappear();

        Task OnAppearedAsync();

        void OnNavigatedFrom(NavigationContext navigationContext);

        void OnNavigatedTo(NavigationContext navigationContext);

        void OnNavigatingBack(BackNavigationContext backNavigationContext);

        #endregion
    }
}
