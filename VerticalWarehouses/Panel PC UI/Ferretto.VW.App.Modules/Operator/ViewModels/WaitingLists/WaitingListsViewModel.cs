using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class WaitingListsViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IAreasDataService areasDataService;

        private readonly IMachineIdentityWebService identityService;

        private readonly IItemListsDataService itemListsDataService;

        private readonly Ferretto.VW.App.Modules.Operator.Interfaces.INavigationService navigationService;

        private readonly IWaitListSelectedModel waitListSelectedModel;

        private int? areaId;

        private int currentItemIndex;

        private ICommand downDataGridButtonCommand;

        private bool isWaitingForResponse;

        private ICommand listDetailButtonCommand;

        private ICommand listExecuteCommand;

        private IList<ItemList> lists;

        private ItemList selectedList;

        private string serialNumber;

        private ICommand upDataGridButtonCommand;

        #endregion

        #region Constructors

        public WaitingListsViewModel(
            IMachineIdentityWebService identityService,
            IItemListsDataService itemListsDataService,
            IAreasDataService areasDataService,
            IWaitListSelectedModel waitListSelectedModel)
            : base(PresentationMode.Operator)
        {
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.itemListsDataService = itemListsDataService ?? throw new ArgumentNullException(nameof(itemListsDataService));
            this.areasDataService = areasDataService ?? throw new ArgumentNullException(nameof(areasDataService));
            this.waitListSelectedModel = waitListSelectedModel ?? throw new ArgumentNullException(nameof(waitListSelectedModel));

            this.lists = new List<ItemList>();
        }

        #endregion

        #region Properties

        public ICommand DownDataGridButtonCommand => this.downDataGridButtonCommand ?? (this.downDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedListAsync(false)));

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set => this.SetProperty(ref this.isWaitingForResponse, value);
        }

        public override bool KeepAlive => true;

        public ICommand ListDetailButtonCommand => this.listDetailButtonCommand ?? (this.listDetailButtonCommand = new DelegateCommand(() => this.Detail(), this.CanDetailCommand));

        public ICommand ListExecuteCommand =>
                    this.listExecuteCommand
            ??
            (this.listExecuteCommand = new DelegateCommand(async () => await this.ExecuteListAsync(), this.CanExecuteList));

        public IList<ItemList> Lists => new List<ItemList>(this.lists);

        public ItemList SelectedList
        {
            get => this.selectedList;
            set
            {
                if (this.SetProperty(ref this.selectedList, value))
                {
                    this.waitListSelectedModel.SelectedList = this.selectedList;

                    ((DelegateCommand)this.ListExecuteCommand).RaiseCanExecuteChanged();
                    ((DelegateCommand)this.ListDetailButtonCommand).RaiseCanExecuteChanged();
                }
            }
        }

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
                if (!this.areaId.HasValue)
                {
                    return;
                }

                await this.itemListsDataService.ExecuteAsync(this.selectedList.Id, this.areaId.Value);
                await this.LoadListsAsync();
            }
            catch
            {
                this.ShowNotification($"Cannot execute List.", Services.Models.NotificationSeverity.Warning);
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;
            if (this.selectedList != null)
            {
                return;
            }

            var machineIdentity = await this.identityService.GetAsync();
            if (machineIdentity == null)
            {
                return;
            }

            this.serialNumber = machineIdentity.SerialNumber;
            this.areaId = machineIdentity.AreaId;
            await this.LoadListsAsync();
        }

        private bool CanDetailCommand()
        {
            return !this.IsWaitingForResponse
                &&
                this.SelectedList != null;
        }

        private bool CanExecuteList()
        {
            if (this.selectedList == null)
            {
                return false;
            }

            if (this.selectedList.Machines.Any(m => m.Id.ToString() == this.serialNumber))
            {
                return true;
            }

            return false;
        }

        private void Detail()
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.WaitingLists.DETAIL,
                    null,
                    trackCurrentView: true);
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task LoadListsAsync()
        {
            if (!this.areaId.HasValue)
            {
                return;
            }

            this.lists = await this.areasDataService.GetItemListsAsync(this.areaId.Value);
            this.RaisePropertyChanged(nameof(this.Lists));
            this.currentItemIndex = 0;
            this.SelectedList = this.lists.FirstOrDefault();
        }

        #endregion
    }
}
