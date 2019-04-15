using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists.ListDetail;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists
{
    public class ListsInWaitViewModel : BindableBase, IListsInWaitViewModel
    {
        #region Fields

        private ICommand detailListButtonCommand;

        private IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public ListsInWaitViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand DetailListButtonCommand => this.detailListButtonCommand ?? (this.detailListButtonCommand = new DelegateCommand(
            () => NavigationService.NavigateToView<DetailListInWaitViewModel, IDetailListInWaitViewModel>()));

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
