using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class WaitingListDetailViewModel : BaseOperatorViewModel, IOperationalContextViewModel
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly IMachineIdentityWebService identityService;

        private readonly IMachineItemListsWebService itemListsWebService;

        private int? areaId;

        private int currentItemIndex;

        private DelegateCommand downCommand;

        private ItemList list;

        private DelegateCommand listExecuteCommand;

        private IEnumerable<ItemListRow> listRows;

        private int machineId;

        private ItemListRow selectedListRow;

        private DelegateCommand upCommand;

        #endregion

        #region Constructors

        public WaitingListDetailViewModel(
            IMachineIdentityWebService identityService,
            IMachineItemListsWebService itemListsWebService,
            IAuthenticationService authenticationService)
            : base(PresentationMode.Operator)
        {
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.itemListsWebService = itemListsWebService ?? throw new ArgumentNullException(nameof(itemListsWebService));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));

            this.listRows = new List<ItemListRow>();
        }

        #endregion

        #region Properties

        public string ActiveContextName => OperationalContext.ListSearch.ToString();

        public ICommand DownCommand =>
            this.downCommand
            ??
            (this.downCommand = new DelegateCommand(() => this.ChangeSelectedListAsync(false), this.CanDown));

        public override EnableMask EnableMask => EnableMask.Any;

        public IMachineItemListsWebService ItemListsWebService { get; }

        public ItemList List => this.list;

        public ICommand ListExecuteCommand =>
            this.listExecuteCommand
            ??
            (this.listExecuteCommand = new DelegateCommand(
                async () => await this.ExecuteListAsync(),
                this.CanExecuteList));

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

            if (this.listRows.Any())
            {
                this.currentItemIndex = isUp ? --this.currentItemIndex : ++this.currentItemIndex;
                if (this.currentItemIndex < 0 || this.currentItemIndex >= this.listRows.Count())
                {
                    this.currentItemIndex = (this.currentItemIndex < 0) ? 0 : this.listRows.Count() - 1;
                }

                this.SelectListRow();
            }
        }

        public async Task CommandUserActionAsync(UserActionEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            if (e.UserAction is UserAction.ExecuteList)
            {
                await this.ExecuteListByBarcodeAsync(e);
            }
        }

        public async Task ExecuteListAsync()
        {
            if (!this.areaId.HasValue || this.list is null)
            {
                return;
            }

            try
            {
                await this.itemListsWebService.ExecuteAsync(this.list.Id, this.areaId.Value, null, this.authenticationService.UserName);
                await this.LoadListRowsAsync();
                this.ShowNotification(
                    string.Format(Resources.Localized.Get("OperatorApp.ExecutionOfListAccepted"), this.list.Code),
                    Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(
                    Resources.Localized.Get("OperatorApp.CannotExecuteList"),
                    Services.Models.NotificationSeverity.Warning);
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
              this.currentItemIndex < this.listRows.Count() - 1;
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

        private async Task ExecuteListByBarcodeAsync(UserActionEventArgs e)
        {
            var listId = e.GetListId();
            if (!listId.HasValue)
            {
                this.ShowNotification(
                   string.Format(Resources.Localized.Get("OperatorApp.BarcodeDoesNotContainTheListId"), e.Code),
                   Services.Models.NotificationSeverity.Warning);

                return;
            }

            if (listId.Value == this.list?.Id)
            {
                await this.ExecuteListAsync();
            }
            else
            {
                this.ShowNotification(
                   string.Format(Resources.Localized.Get("OperatorApp.BarcodeDoesNotContainTheListId"), e.Code),
                   Services.Models.NotificationSeverity.Warning);

                return;
            }
        }

        private async Task LoadListRowsAsync()
        {
            try
            {
                this.listRows = await this.itemListsWebService.GetRowsAsync(this.list.Id);
                this.RaisePropertyChanged(nameof(this.ListRows));
                this.SelectedListRow = this.listRows.FirstOrDefault();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
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
