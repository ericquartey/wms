using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        private int? currentItemIndex;

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

        public ICommand DownCommand => this.downCommand
            ?? (this.downCommand = new DelegateCommand(
            () => this.ChangeSelectedList(false),
            this.CanSelectNext));

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set => this.SetProperty(ref this.isWaitingForResponse, value);
        }

        public override bool KeepAlive => true;

        public ICommand ListDetailButtonCommand => this.listDetailButtonCommand ?? (this.listDetailButtonCommand = new DelegateCommand(() => this.ShowDetails(), this.CanShowDetails));

        public ICommand ListExecuteCommand =>
            this.listExecuteCommand
            ??
            (this.listExecuteCommand = new DelegateCommand(
                async () => await this.ExecuteListAsync(),
                this.CanExecuteList));

        public IList<ItemListExecution> Lists => new List<ItemListExecution>(this.lists);

        public ItemListExecution SelectedList
        {
            get => this.selectedList;
            set => this.SetProperty(ref this.selectedList, value, this.RaiseCanExecuteChanged);
        }

        public ICommand UpCommand => this.upCommand
            ??
            (this.upCommand = new DelegateCommand(
                () => this.ChangeSelectedList(true),
                this.CanSelectPrevious));

        private int? CurrentItemIndex
        {
            get => this.currentItemIndex;
            set
            {
                if (this.SetProperty(ref this.currentItemIndex, value))
                {
                    if (this.currentItemIndex.HasValue)
                    {
                        this.SelectedList = this.lists.ElementAtOrDefault(this.currentItemIndex.Value);
                    }
                    else
                    {
                        this.SelectedList = null;
                    }
                }
            }
        }

        #endregion

        #region Methods

        public void ChangeSelectedList(bool isUp)
        {
            if (this.lists is null || !this.CurrentItemIndex.HasValue)
            {
                return;
            }

            if (this.lists.Any())
            {
                var newIndex = isUp ? this.CurrentItemIndex.Value - 1 : this.CurrentItemIndex.Value + 1;

                this.CurrentItemIndex = Math.Max(0, Math.Min(newIndex, this.lists.Count - 1));
            }
        }

        public async Task ExecuteListAsync()
        {
            System.Diagnostics.Debug.Assert(this.areaId.HasValue, "The area should be specified");

            try
            {
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
            if (machineIdentity is null)
            {
                return;
            }

            this.machineId = machineIdentity.Id;
            this.areaId = machineIdentity.AreaId;

            await Task.Run(async () =>
            {
                do
                {
                    await this.LoadListsAsync();
                    await Task.Delay(5000);
                } while (this.IsVisible);
            });
        }

        private bool CanExecuteList()
        {
            return
                this.areaId.HasValue
                &&
                !this.IsWaitingForResponse
                &&
                this.SelectedList != null
                &&
                this.SelectedList.ExecutionMode != ListExecutionMode.None;
        }

        private bool CanSelectNext()
        {
            return
                this.CurrentItemIndex < this.lists.Count - 1
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanSelectPrevious()
        {
            return
                this.CurrentItemIndex > 0
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanShowDetails()
        {
            return
                !this.IsWaitingForResponse
                &&
                this.SelectedList != null;
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
                var newLists = await this.areasDataService.GetItemListsAsync(this.areaId.Value);
                this.IsWaitingForResponse = false;

                var commonListsCount = newLists.Select(l => l.Id).Intersect(this.lists.Select(l => l.Id)).Count();
                if (commonListsCount == this.lists.Count && commonListsCount == newLists.Count)
                {
                    return;
                }

                this.lists.Clear();
                newLists.ForEach(l => this.lists.Add(new ItemListExecution(l, this.machineId)));

                this.RaisePropertyChanged(nameof(this.Lists));
                this.RaiseCanExecuteChanged();
                this.SetCurrentIndex(lastItemListId);
            }
            catch (Exception ex)
            {
                this.Lists?.Clear();
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

        private void SetCurrentIndex(int? itemListId)
        {
            if (itemListId.HasValue
                &&
                this.lists.FirstOrDefault(l => l.Id == itemListId.Value) is ItemListExecution foundList)
            {
                this.CurrentItemIndex = this.Lists.IndexOf(foundList);
            }
            else
            {
                this.CurrentItemIndex = 0;
            }
        }

        private void ShowDetails()
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

        #endregion
    }
}
