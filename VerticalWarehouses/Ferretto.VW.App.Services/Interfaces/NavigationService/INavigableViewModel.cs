using System;
using Prism.Regions;

namespace Ferretto.VW.App.Services.Interfaces
{
    public interface INavigableViewModel : IDisposable
    {
        #region Properties

        bool IsBusy { get; set; }

        #endregion

        #region Methods

        void Appear();

        bool CanClose();

        void Disappear();

        void OnNavigated();

        void OnNavigatedFrom(NavigationContext navigationContext);

        void OnNavigatedTo(NavigationContext navigationContext);

        #endregion
    }
}
