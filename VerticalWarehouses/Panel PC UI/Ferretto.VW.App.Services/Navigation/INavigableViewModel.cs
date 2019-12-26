using System;
using System.Threading.Tasks;
using Prism.Regions;

namespace Ferretto.VW.App.Services
{
    public interface INavigableViewModel : IDisposable
    {
        #region Methods

        bool CanClose();

        void Disappear();

        Task OnAppearedAsync();

        void OnNavigatedFrom(NavigationContext navigationContext);

        void OnNavigatedTo(NavigationContext navigationContext);

        bool IsVisible { get; }

        #endregion
    }
}
