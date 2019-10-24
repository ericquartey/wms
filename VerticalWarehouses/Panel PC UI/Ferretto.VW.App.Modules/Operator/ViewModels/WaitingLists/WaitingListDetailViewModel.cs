using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class WaitingListDetailViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineIdentityWebService identityService;

        private readonly IItemListsDataService itemListsDataService;

        private readonly IWaitListSelectedModel waitListSelectedModel;

        private int areaId;

        private int currentItemIndex;

        private ICommand downDataGridButtonCommand;

        private ItemList list;

        private ICommand listExecuteCommand;

        private IList<ItemListRow> listRows;

        private string machineId;

        private ItemListRow selectedListRow;

        private ICommand upDataGridButtonCommand;

        #endregion

        #region Constructors

        public WaitingListDetailViewModel(
            IMachineIdentityWebService identityService,
            IItemListsDataService itemListsDataService,
            IWaitListSelectedModel waitListSelectedModel)
            : base(PresentationMode.Operator)
        {
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.itemListsDataService = itemListsDataService ?? throw new ArgumentNullException(nameof(itemListsDataService));
            this.waitListSelectedModel = waitListSelectedModel ?? throw new ArgumentNullException(nameof(waitListSelectedModel));

            this.listRows = new List<ItemListRow>();
        }

        #endregion

        #region Properties

        public ICommand DownDataGridButtonCommand =>
            this.downDataGridButtonCommand
            ??
            (this.downDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedListAsync(false)));

        public override EnableMask EnableMask => EnableMask.None;

        public IItemListsDataService ItemListsDataService { get; }

        public ItemList List => this.list;

        public ICommand ListExecuteCommand =>
            this.listExecuteCommand
            ??
            (this.listExecuteCommand = new DelegateCommand(async () => await this.ExecuteListAsync(), this.CanExecuteList));

        public IList<ItemListRow> ListRows => new List<ItemListRow>(this.listRows);

        public string MachineId => this.machineId;

        public ItemListRow SelectedListRow
        {
            get => this.selectedListRow;
            set => this.SetProperty(ref this.selectedListRow, value);
        }

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
                this.ShowNotification("Cannot execute List.", Services.Models.NotificationSeverity.Warning);
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            var machineIdentity = await this.identityService.GetAsync();
            if (machineIdentity == null)
            {
                return;
            }

            if (this.waitListSelectedModel == null &&
                this.waitListSelectedModel.SelectedList == null)
            {
                return;
            }

            this.list = this.waitListSelectedModel.SelectedList;
            this.RaisePropertyChanged(nameof(this.List));

            this.machineId = machineIdentity.SerialNumber;
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

            if (this.ListRows.Any(r => r.Machines.Any(m => m.Id.ToString() == this.machineId)))
            {
                return true;
            }

            return false;
        }

        private async Task LoadListRowsAsync()
        {
            try
            {
                this.listRows = await this.itemListsDataService.GetRowsAsync(this.list.Id);
                this.RaisePropertyChanged(nameof(this.ListRows));
                this.currentItemIndex = 0;
                this.SelectedListRow = this.listRows.FirstOrDefault();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex.ToString(), Services.Models.NotificationSeverity.Error);
            }
        }

        #endregion
    }
}
