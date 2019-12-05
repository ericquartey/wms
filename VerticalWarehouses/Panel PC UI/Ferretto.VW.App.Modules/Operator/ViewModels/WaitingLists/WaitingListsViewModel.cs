using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Modules.Operator.Models;
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

        private readonly IBayManager bayManager;

        private readonly IMachineIdentityWebService identityService;

        private readonly IItemListsDataService itemListsDataService;

        private readonly IList<ItemListExecution> lists;

        private int? areaId;

        private int currentItemIndex;

        private DelegateCommand downCommand;

        private bool isWaitingForResponse;

        private DelegateCommand listDetailButtonCommand;

        private DelegateCommand listExecuteCommand;

        private int machineId;

        private ItemListExecution selectedList;

        private DelegateCommand upCommand;

        #endregion

        #region Constructors

        public WaitingListsViewModel(
            IMachineIdentityWebService identityService,
            IItemListsDataService itemListsDataService,
            IAreasDataService areasDataService,
            IBayManager bayManager)
            : base(PresentationMode.Operator)
        {
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.itemListsDataService = itemListsDataService ?? throw new ArgumentNullException(nameof(itemListsDataService));
            this.areasDataService = areasDataService ?? throw new ArgumentNullException(nameof(areasDataService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.lists = new List<ItemListExecution>();
        }

        #endregion

        #region Properties

        public ICommand DownCommand => this.downCommand ?? (this.downCommand = new DelegateCommand(() => this.ChangeSelectedListAsync(false), this.CanDown));

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

        public IList<ItemListExecution> Lists => new List<ItemListExecution>(this.lists);

        public ItemListExecution SelectedList
        {
            get => this.selectedList;
            set
            {
                if (this.SetProperty(ref this.selectedList, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand UpCommand => this.upCommand ?? (this.upCommand = new DelegateCommand(() => this.ChangeSelectedListAsync(true), this.CanUp));

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

            this.SelectLoadingUnit();
        }

        public async Task ExecuteListAsync()
        {
            try
            {
                if (!this.areaId.HasValue)
                {
                    return;
                }
                var bay = await this.bayManager.GetBayAsync();
                await this.itemListsDataService.ExecuteAsync(this.selectedList.Id, this.areaId.Value, bay.Id);
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

            var machineIdentity = await this.identityService.GetAsync();
            if (machineIdentity == null)
            {
                return;
            }

            this.machineId = machineIdentity.Id;
            this.areaId = machineIdentity.AreaId;

            await this.LoadListsAsync();
        }

        private bool CanDetailCommand()
        {
            return !this.IsWaitingForResponse
                &&
                this.SelectedList != null;
        }

        private bool CanDown()
        {
            return
                this.currentItemIndex < this.lists.Count - 1;
        }

        private bool CanExecuteList()
        {
            if (this.selectedList == null)
            {
                return false;
            }
            if (this.selectedList.ExecutionMode != ListExecutionMode.None)
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

        private void Detail()
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.WaitingLists.DETAIL,
                    this.selectedList,
                    trackCurrentView: true);
            }
            catch (Exception ex)
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

            try
            {
                this.IsWaitingForResponse = true;

                var lastItemListId = this.selectedList?.Id;
                var lists = await this.areasDataService.GetItemListsAsync(this.areaId.Value);

                this.lists.Clear();
                lists.ForEach(l => this.lists.Add(new ItemListExecution(l, this.machineId)));

                this.RaisePropertyChanged(nameof(this.Lists));

                this.SetCurrentIndex(lastItemListId);

                this.SelectLoadingUnit();
            }
            catch (Exception ex)
            {
                this.lists?.Clear();
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.upCommand?.RaiseCanExecuteChanged();
            this.downCommand?.RaiseCanExecuteChanged();
            this.listExecuteCommand?.RaiseCanExecuteChanged();
            this.listDetailButtonCommand.RaiseCanExecuteChanged();
        }

        private void SelectLoadingUnit()
        {
            this.SelectedList = this.lists.ElementAt(this.currentItemIndex);
            this.RaiseCanExecuteChanged();
        }

        private void SetCurrentIndex(int? itemListId)
        {
            if (itemListId.HasValue
                &&
                this.lists.FirstOrDefault(l => l.Id == itemListId.Value) is ItemListExecution itemListFound)
            {
                this.currentItemIndex = this.Lists.IndexOf(itemListFound);
            }
            else
            {
                this.currentItemIndex = 0;
            }
        }

        #endregion
    }
}
