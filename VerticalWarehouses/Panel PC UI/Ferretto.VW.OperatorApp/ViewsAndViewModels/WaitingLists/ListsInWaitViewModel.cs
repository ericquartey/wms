using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Operator.Interfaces;
using Ferretto.VW.App.Operator.ViewsAndViewModels.WaitingLists.ListDetail;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.WmsCommunication.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewsAndViewModels.WaitingLists
{
    public class ListsInWaitViewModel : BaseViewModel, IListsInWaitViewModel
    {
        #region Fields

        private readonly IMachineProvider machineProvider;

        private readonly INavigationService navigationService;

        private readonly IWmsDataProvider wmsDataProvider;

        private int areaId;

        private int currentItemIndex;

        private ICommand downDataGridButtonCommand;

        private ICommand itemDetailButtonCommand;

        private ICommand listExecuteCommand;

        private IList<ItemList> lists;

        private int machineId;

        private ItemList selectedList;

        private ICommand upDataGridButtonCommand;

        #endregion

        #region Constructors

        public ListsInWaitViewModel(
            IStatusMessageService statusMessageService,
            IWmsDataProvider wmsDataProvider,
            INavigationService navigationService,
            IMachineProvider machineProvider
            )
        {
            if (statusMessageService == null)
            {
                throw new ArgumentNullException(nameof(statusMessageService));
            }

            if (wmsDataProvider == null)
            {
                throw new ArgumentNullException(nameof(wmsDataProvider));
            }

            if (navigationService == null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            this.StatusMessageService = statusMessageService;
            this.machineProvider = machineProvider;
            this.wmsDataProvider = wmsDataProvider;
            this.navigationService = navigationService;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand DownDataGridButtonCommand => this.downDataGridButtonCommand ?? (this.downDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedListAsync(false)));

        public ICommand ItemDetailButtonCommand =>
            this.itemDetailButtonCommand
            ??
            (this.itemDetailButtonCommand = new DelegateCommand(() =>
            {
                if (this.SelectedList != null)
                {
                    this.navigationService.NavigateToView<DetailListInWaitViewModel, IDetailListInWaitViewModel>(this.selectedList);
                }
            },
            this.CanShowDetails));

        public ICommand ListExecuteCommand => this.listExecuteCommand ?? (this.listExecuteCommand = new DelegateCommand(() => this.ExecuteListAsync(), this.CanExecuteList));

        public IList<ItemList> Lists { get => new List<ItemList>(this.lists); }

        public ItemList SelectedList
        {
            get => this.selectedList;
            set
            {
                if (this.SetProperty(ref this.selectedList, value))
                {
                    ((DelegateCommand)this.ListExecuteCommand).RaiseCanExecuteChanged();
                    ((DelegateCommand)this.ItemDetailButtonCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public IStatusMessageService StatusMessageService { get; }

        public ICommand UpDataGridButtonCommand => this.upDataGridButtonCommand ?? (this.upDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedListAsync(true)));

        #endregion

        #region Methods

        public void ChangeSelectedListAsync(bool isUp)
        {
            if (this.lists == null)
            {
                return;
            }

            if (this.lists.Count() != 0)
            {
                this.currentItemIndex = isUp ? --this.currentItemIndex : ++this.currentItemIndex;
                if (this.currentItemIndex < 0 || this.currentItemIndex >= this.lists.Count())
                {
                    this.currentItemIndex = (this.currentItemIndex < 0) ? 0 : this.lists.Count() - 1;
                }

                this.SelectedList = this.lists[this.currentItemIndex];
            }
        }

        public async Task ExecuteListAsync()
        {
            try
            {
                await this.wmsDataProvider.ItemListExecute(this.selectedList.Id, this.areaId);
                await this.LoadListsAsync();
            }
            catch (Exception ex)
            {
                this.StatusMessageService.Notify(ex, $"Cannot execute List.");
            }
        }

        public override async Task OnEnterViewAsync()
        {
            if (this.selectedList != null)
            {
                return;
            }

            var machineIdentity = await this.machineProvider.GetIdentityAsync();
            if (machineIdentity == null)
            {
                return;
            }

            this.machineId = machineIdentity.Id;
            this.areaId = machineIdentity.AreaId;
            await this.LoadListsAsync();
        }

        private bool CanExecuteList()
        {
            if (this.selectedList == null)
            {
                return false;
            }

            if (this.selectedList.Machines.Any(m => m.Id == this.machineId))
            {
                return true;
            }

            return false;
        }

        private bool CanShowDetails()
        {
            return this.SelectedList != null;
        }

        private async Task LoadListsAsync()
        {
            this.lists = await this.wmsDataProvider.GetItemLists(this.areaId);
            this.RaisePropertyChanged(nameof(this.Lists));
            this.currentItemIndex = 0;
            this.SelectedList = this.lists.FirstOrDefault();
        }

        #endregion
    }
}
