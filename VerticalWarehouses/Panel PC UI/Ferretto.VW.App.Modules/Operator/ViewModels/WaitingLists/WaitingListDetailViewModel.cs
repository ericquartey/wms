using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class WaitingListDetailViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineIdentityWebService identityService;

        private readonly IItemListsDataService itemListsDataService;

        private int? areaId;

        private int currentItemIndex;

        private DelegateCommand downCommand;

        private ItemList list;

        private DelegateCommand listExecuteCommand;

        private IList<ItemListRow> listRows;

        private int machineId;

        private ItemListRow selectedListRow;

        private DelegateCommand upCommand;

        #endregion

        #region Constructors

        public WaitingListDetailViewModel(
            IMachineIdentityWebService identityService,
            IItemListsDataService itemListsDataService)
            : base(PresentationMode.Operator)
        {
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.itemListsDataService = itemListsDataService ?? throw new ArgumentNullException(nameof(itemListsDataService));

            this.listRows = new List<ItemListRow>();
        }

        #endregion

        #region Properties

        public ICommand DownCommand =>
            this.downCommand
            ??
            (this.downCommand = new DelegateCommand(() => this.ChangeSelectedListAsync(false), this.CanDown));

        public override EnableMask EnableMask => EnableMask.Any;

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

        public ICommand UpCommand =>
            this.upCommand
            ??
            (this.upCommand = new DelegateCommand(() => this.ChangeSelectedListAsync(true), this.CanUp));

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

                this.SelectListRow();
            }
        }

        public async Task ExecuteListAsync()
        {
            try
            {
                if (!this.areaId.HasValue
                     ||
                     this.selectedListRow == null)
                {
                    return;
                }

                await this.itemListsDataService.ExecuteAsync(this.selectedListRow.Id, this.areaId.Value);
                await this.LoadListRowsAsync();
            }
            catch
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

            if (this.Data is ItemList list)
            {
                this.list = list;
            }

            this.RaisePropertyChanged(nameof(this.List));

            this.machineId = machineIdentity.Id;
            this.areaId = machineIdentity.AreaId;

            await this.LoadListRowsAsync();

            ((DelegateCommand)this.ListExecuteCommand).RaiseCanExecuteChanged();
            this.SelectListRow();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.upCommand?.RaiseCanExecuteChanged();
            this.downCommand?.RaiseCanExecuteChanged();
            this.listExecuteCommand?.RaiseCanExecuteChanged();
        }

        private bool CanDown()
        {
            return
              this.currentItemIndex < this.listRows.Count - 1;
        }

        private bool CanExecuteList()
        {
            if (this.list == null
                ||
                this.ListRows == null)
            {
                return false;
            }

            if (this.ListRows.Any(r => r.Machines.Any(m => m.Id == this.machineId)))
            {
                return true;
            }

            return false;
        }

        private bool CanUp()
        {
            return
                this.currentItemIndex > 0;
        }

        private async Task LoadListRowsAsync()
        {
            try
            {
                this.listRows = await this.itemListsDataService.GetRowsAsync(this.list.Id);
                this.RaisePropertyChanged(nameof(this.ListRows));
                this.SelectedListRow = this.listRows.FirstOrDefault();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex.ToString(), Services.Models.NotificationSeverity.Error);
            }
        }

        private void SelectListRow()
        {
            this.SelectedListRow = this.listRows.ElementAt(this.currentItemIndex);
            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
