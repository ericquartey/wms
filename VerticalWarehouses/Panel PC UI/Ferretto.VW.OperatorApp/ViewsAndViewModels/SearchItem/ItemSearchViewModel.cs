using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.SearchItem
{
    public class ItemSearchViewModel : BindableBase, IItemSearchViewModel
    {
        #region Fields

        private IEventAggregator eventAggregator;

        private ICommand itemDetailButtonCommand;

        private IUnityContainer container;


        #endregion

        #region Constructors

        public ItemSearchViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand ItemDetailButtonCommand => this.itemDetailButtonCommand ?? (this.itemDetailButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.NavigateToView<ItemDetailViewModel, IItemDetailViewModel>();
        }));

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task OnEnterViewAsync()
        {
            // TODO
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
