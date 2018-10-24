using System.Collections.Generic;
using System.Windows.Input;
using Ferretto.VW.VerticalWarehousesApp.Source;

namespace Ferretto.VW.VerticalWarehousesApp.ViewModels
{
    internal class AppViewModel
    {
        #region Fields

        private ICommand _changeViewCommand;

        private IViewViewModel _currentViewViewModel;
        private List<IViewViewModel> _viewViewModels;

        #endregion Fields

        #region Constructors

        public AppViewModel()
        {
            this.ViewViewModels.Add(new TestConnectionPageViewModel());
            this.ViewViewModels.Add(new CompartmentationPageViewModel());
        }

        #endregion Constructors

        #region Properties

        public ICommand ChangePageCommand { get => this._changeViewCommand; set => this._changeViewCommand = value; }
        public IViewViewModel CurrentViewViewModel { get => this._currentViewViewModel; set => this._currentViewViewModel = value; }
        public List<IViewViewModel> ViewViewModels { get => this._viewViewModels; set => this._viewViewModels = value; }

        #endregion Properties
    }
}
