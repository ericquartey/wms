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

        Task OnInitializedAsync();

        void OnNavigatedFrom(NavigationContext navigationContext);

        void OnNavigatedTo(NavigationContext navigationContext);

        #endregion
    }
}
