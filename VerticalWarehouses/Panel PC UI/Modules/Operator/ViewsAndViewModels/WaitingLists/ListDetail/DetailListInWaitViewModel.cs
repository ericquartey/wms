using CommonServiceLocator;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.WaitingLists.ListDetail
{
    public class DetailListInWaitViewModel : BaseViewModel, IDetailListInWaitViewModel
    {
        #region Fields

        private readonly IIdentityMachineService identityService;

        private readonly IItemListsDataService itemListsDataService;

        private readonly Ferretto.VW.App.Modules.Operator.Interfaces.INavigationService navigationService;

        private int areaId;

        private int currentItemIndex;

        private ICommand downDataGridButtonCommand;

        private ItemList list;

        private ICommand listExecuteCommand;

        private IList<ItemListRow> listRows;

        private int machineId;

        private ItemListRow selectedListRow;

        private ICommand upDataGridButtonCommand;

        #endregion

        #region Constructors

        public DetailListInWaitViewModel(
            IStatusMessageService statusMessageService,
            Ferretto.VW.App.Modules.Operator.Interfaces.INavigationService navigationService,
            IIdentityMachineService identityService,
            IItemListsDataService itemListsDataService)
        {
            if (statusMessageService == null)
            {
                throw new ArgumentNullException(nameof(statusMessageService));
            }

            if (itemListsDataService == null)
            {
                throw new ArgumentNullException(nameof(itemListsDataService));
            }

            if (identityService == null)
            {
                throw new ArgumentNullException(nameof(identityService));
            }

            if (navigationService == null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            this.StatusMessageService = statusMessageService;
            this.navigationService = navigationService;
            this.identityService = identityService;
            this.itemListsDataService = itemListsDataService;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand DownDataGridButtonCommand =>
            this.downDataGridButtonCommand
            ??
            (this.downDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedListAsync(false)));

        public IItemListsDataService ItemListsDataService { get; }

        public ItemList List => this.list;

        public ICommand ListExecuteCommand =>
            this.listExecuteCommand
            ??
            (this.listExecuteCommand = new DelegateCommand(async () => await this.ExecuteListAsync(), this.CanExecuteList));

        public IList<ItemListRow> ListRows => new List<ItemListRow>(this.listRows);

        public int MachineId => this.machineId;

        public ItemListRow SelectedListRow
        {
            get => this.selectedListRow;
            set => this.SetProperty(ref this.selectedListRow, value);
        }

        public IStatusMessageService StatusMessageService { get; }

        public ICommand UpDataGridButtonCommand =>
            this.upDataGridButtonCommand
            ??
            (this.upDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedListAsync(true)));

        #endregion

        #region Methods

        public void ChangeSelectedListAsync(bool isUp)
        {
            if (this.listRows == null)
            {
                return;
            }

            if (this.listRows.Count() != 0)
            {
                this.currentItemIndex = isUp ? --this.currentItemIndex : ++this.currentItemIndex;
                if (this.currentItemIndex < 0 || this.currentItemIndex >= this.listRows.Count())
                {
                    this.currentItemIndex = (this.currentItemIndex < 0) ? 0 : this.listRows.Count() - 1;
                }

                this.SelectedListRow = this.listRows[this.currentItemIndex];
            }
        }

        public async Task ExecuteListAsync()
        {
            try
            {
                await this.itemListsDataService.ExecuteAsync(this.list.Id, this.areaId);
                await this.LoadListRowsAsync();
            }
            catch (Exception ex)
            {
                this.StatusMessageService.Notify(ex, "Cannot execute List.");
            }
        }

        public override async Task OnEnterViewAsync()
        {
            var machineIdentity = await this.identityService.GetAsync();
            if (machineIdentity == null)
            {
                return;
            }

            var listInWaitViewModel = ServiceLocator.Current.GetInstance<IListsInWaitViewModel>();
            if (listInWaitViewModel == null &&
                listInWaitViewModel.SelectedList == null)
            {
                return;
            }

            this.list = listInWaitViewModel.SelectedList;
            this.RaisePropertyChanged(nameof(this.List));

            this.machineId = machineIdentity.Id;
            this.areaId = machineIdentity.AreaId;

            await this.LoadListRowsAsync();

            ((DelegateCommand)this.ListExecuteCommand).RaiseCanExecuteChanged();
        }

        private bool CanExecuteList()
        {
            if (this.ListRows == null)
            {
                return false;
            }

            if (this.ListRows.Any(r => r.Machines.Any(m => m.Id == this.machineId)))
            {
                return true;
            }

            return false;
        }

        private async Task LoadListRowsAsync()
        {
            this.listRows = await this.itemListsDataService.GetRowsAsync(this.list.Id);
            this.RaisePropertyChanged(nameof(this.ListRows));
            this.currentItemIndex = 0;
            this.SelectedListRow = this.listRows.FirstOrDefault();
        }

        #endregion
    }
}
