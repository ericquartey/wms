using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.VerticalWarehousesApp.ViewModels
{
    class AppViewModel 
    {
        private ICommand _changeViewCommand;

        private IViewViewModel _currentViewViewModel;
        private List<IViewViewModel> _viewViewModels;        

        public ICommand ChangePageCommand { get => this._changeViewCommand; set => this._changeViewCommand = value; }
        public IViewViewModel CurrentViewViewModel { get => this._currentViewViewModel; set => this._currentViewViewModel = value; }
        public List<IViewViewModel> ViewViewModels { get => this._viewViewModels; set => this._viewViewModels = value; }

        public AppViewModel()
        {
            this.ViewViewModels.Add(new TestConnectionPageViewModel());
            this.ViewViewModels.Add(new RandomPageViewModel());

            this.CurrentViewViewModel = this.ViewViewModels[1];
        }

        
    }
}
