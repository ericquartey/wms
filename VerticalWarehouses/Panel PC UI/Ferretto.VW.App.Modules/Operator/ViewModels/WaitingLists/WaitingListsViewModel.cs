﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Microsoft.AspNetCore.Http;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.User)]
    public class WaitingListsViewModel : BaseOperatorViewModel, IOperationalContextViewModel
    {
        #region Fields

        private const int PollIntervalMilliseconds = 60000;

        private readonly IMachineAreasWebService areasWebService;

        private readonly IAuthenticationService authenticationService;

        private readonly IBayManager bayManager;

        private readonly IMachineIdentityWebService identityService;

        private readonly IMachineItemListsWebService itemListsWebService;

        private readonly List<ItemListExecution> lists = new List<ItemListExecution>();

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private readonly ISessionService sessionService;

        private int? areaId;

        private int currentItemIndex;

        private bool isCarrefour;

        private bool isShipmentDayVisible;

        private bool? isWaitingListLocal;

        private string itemSearchLabel;

        private DelegateCommand listDetailButtonCommand;

        private DelegateCommand listExecuteCommand;

        private int machineId;

        private int? priorityHighlighted;

        private bool reloadSearchItems;

        private string searchItem;

        private ItemListExecution selectedList;

        private List<ItemListExecution> selectedLists = new List<ItemListExecution>();

        private DelegateCommand selectOperationOnBayCommand;

        private DelegateCommand showExecutingListCommand;

        private bool showOnlyExecutingList;

        private DelegateCommand showWaitingListCommand;

        private DelegateCommand suspendCommand;

        private CancellationTokenSource tokenSource;

        #endregion

        #region Constructors

        public WaitingListsViewModel(
            IMachineIdentityWebService identityService,
            IMachineItemListsWebService itemListsWebService,
            IMachineAreasWebService areasWebService,
            IMachineConfigurationWebService machineConfigurationWebService,
            IBayManager bayManager,
            IAuthenticationService authenticationService,
            ISessionService sessionService)
            : base(PresentationMode.Operator)
        {
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.itemListsWebService = itemListsWebService ?? throw new ArgumentNullException(nameof(itemListsWebService));
            this.areasWebService = areasWebService ?? throw new ArgumentNullException(nameof(areasWebService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
        }

        #endregion

        #region Properties

        public string ActiveContextName => OperationalContext.ListSearch.ToString();

        public bool IsCarrefour
        {
            get => this.isCarrefour;
            set => this.SetProperty(ref this.isCarrefour, value);
        }

        public bool IsShipmentDayVisible
        {
            get => this.isShipmentDayVisible;
            protected set => this.SetProperty(ref this.isShipmentDayVisible, value, this.RaiseCanExecuteChanged);
        }

        public string ItemSearchLabel
        {
            get => this.itemSearchLabel;
            set => this.SetProperty(ref this.itemSearchLabel, value);
        }

        public override bool KeepAlive => true;

        public ICommand ListDetailButtonCommand =>
            this.listDetailButtonCommand
            ??
            (this.listDetailButtonCommand = new DelegateCommand(
                () => this.ShowDetails(this.SelectedLists.LastOrDefault()),
                this.CanShowDetailCommand));

        public ICommand ListExecuteCommand =>
            this.listExecuteCommand
            ??
            (this.listExecuteCommand = new DelegateCommand(
                async () => await this.ExecuteListAsync(this.SelectedLists),
                this.CanExecuteList));

        public IList<ItemListExecution> Lists => new List<ItemListExecution>(this.lists);

        public string SearchItem
        {
            get => this.searchItem;
            set
            {
                if (this.SetProperty(ref this.searchItem, value)
                    && this.reloadSearchItems)
                {
                    this.ItemSearchLabel = Localized.Get(OperatorApp.ItemSearchKeySearch);
                    this.TriggerSearchAsync().GetAwaiter();
                }
                this.reloadSearchItems = true;
            }
        }

        public ItemListExecution SelectedList
        {
            get => this.selectedList;
            set => this.SetProperty(ref this.selectedList, value, this.RaiseCanExecuteChanged);
        }

        public List<ItemListExecution> SelectedLists
        {
            get => this.selectedLists;
            set => this.SetProperty(ref this.selectedLists, value, this.RaiseCanExecuteChanged);
        }

        public ICommand SelectOperationOnBayCommand =>
                                    this.selectOperationOnBayCommand
            ??
            (this.selectOperationOnBayCommand = new DelegateCommand(
                () => this.ShowOperationOnBay(),
                this.CanSelectOperationOnBay));

        public ICommand ShowExecutingListCommand => this.showExecutingListCommand
            ??
            (this.showExecutingListCommand = new DelegateCommand(async () => await this.ChangeActiveList(true), this.CanChangeActiveList));

        public bool ShowOnlyExecutingList
        {
            get => this.showOnlyExecutingList;
            set => this.SetProperty(ref this.showOnlyExecutingList, value, this.RaiseCanExecuteChanged);
        }

        public ICommand ShowWaitingListCommand => this.showWaitingListCommand
            ??
            (this.showWaitingListCommand = new DelegateCommand(async () => await this.ChangeActiveList(false), this.CanChangeActiveList));

        public ICommand SuspendCommand => this.suspendCommand
                    ??
            (this.suspendCommand = new DelegateCommand(
                async () => await this.SuspendListAsync(this.SelectedLists), this.CanSuspendList));

        #endregion

        #region Methods

        public void ChangeSelectedList(bool selectPrevious)
        {
            if (this.lists is null)
            {
                return;
            }

            if (this.lists.Any())
            {
                var newIndex = selectPrevious ? this.currentItemIndex - 1 : this.currentItemIndex + 1;

                this.currentItemIndex = Math.Max(0, Math.Min(newIndex, this.lists.Count - 1));
            }

            this.SelectLoadingUnit();
        }

        public async Task CommandUserActionAsync(UserActionEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            switch (e.UserAction)
            {
                case UserAction.FilterLists:

                    await this.FilterListsByBarcodeAsync(e);
                    break;

                case UserAction.ExecuteList:

                    await this.ExecuteListByBarcodeAsync(e);
                    break;
            }
        }

        public override void Disappear()
        {
            base.Disappear();

            this.SearchItem = string.Empty;
            this.lists.Clear();
            this.RaisePropertyChanged(nameof(this.Lists));
            this.IsWaitingForResponse = false;
            this.ShowOnlyExecutingList = false;
        }

        public async Task ExecuteListAsync(List<ItemListExecution> SelectedCells)
        {
            foreach (var itemList in SelectedCells.OrderBy(c => c.Priority))
            {
                if (!this.areaId.HasValue)
                {
                    return;
                }

                if (itemList == null)
                {
                    return;
                }

                // Handle the current not dispatchable list
                if (!itemList.IsDispatchable)
                {
                    this.Logger.Debug($"Show the evadability options view for item list {itemList.Id}");

                    var bay = this.MachineService.Bay;
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.WaitingLists.EVADABILITYOPTIONS,
                        new WaitingListExecuteData
                        {
                            ListId = itemList.Id,
                            ListDescription = itemList.Description,
                            BayId = bay.Id,
                            AreaId = this.areaId.Value,
                            AuthenticationUserName = this.authenticationService.UserName,
                        },
                        trackCurrentView: true);

                    return;
                }

                try
                {
                    var bay = this.MachineService.Bay;
                    await this.itemListsWebService.ExecuteAsync(itemList.Id, this.areaId.Value, ItemListEvadabilityType.Execute, bay.Id, this.authenticationService.UserName);
                    await this.LoadListsAsync();
                    this.ShowNotification(string.Format(Localized.Get("OperatorApp.ExecutionOfListAccepted"), itemList.Code), Services.Models.NotificationSeverity.Success);
                }
                catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                {
                    if (ex is MasWebApiException webEx
                        && webEx.StatusCode == StatusCodes.Status403Forbidden)
                    {
                        this.ShowNotification(Resources.Localized.Get("General.ForbiddenOperation"), Services.Models.NotificationSeverity.Error);
                    }
                    else
                    {
                        this.ShowNotification(
                            Resources.Localized.Get("OperatorApp.CannotExecuteList"),
                            Services.Models.NotificationSeverity.Warning);
                    }
                }
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.SearchItem = string.Empty;

            this.IsBackNavigationAllowed = true;
            this.IsShipmentDayVisible = false;

            var machineIdentity = this.sessionService.MachineIdentity;
            if (machineIdentity is null)
            {
                machineIdentity = await this.identityService.GetAsync();
            }
            if (machineIdentity is null)
            {
                return;
            }

            this.machineId = machineIdentity.Id;
            this.areaId = machineIdentity.AreaId;

            var configuration = await this.machineConfigurationWebService.GetConfigAsync();
            this.IsCarrefour = configuration.IsCarrefour;
            this.priorityHighlighted = configuration.WaitingListPriorityHighlighted;

            this.isWaitingListLocal = configuration.IsWaitingListFiltered;

            await this.LoadListsAsync();

            await this.RefreshListsAsync();

            this.SelectedLists.Clear();
        }

        public async Task SuspendListAsync(List<ItemListExecution> SelectedCells)
        {
            foreach (var itemList in SelectedCells)
            {
                if (!this.areaId.HasValue)
                {
                    return;
                }

                if (itemList == null)
                {
                    return;
                }

                try
                {
                    await this.itemListsWebService.SuspendAsync(itemList.Id, this.authenticationService.UserName);
                    await this.LoadListsAsync();
                    this.ShowNotification(string.Format(Localized.Get("OperatorApp.SuspendOfListAccepted"), itemList.Code), Services.Models.NotificationSeverity.Success);
                }
                catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                {
                    if (ex is MasWebApiException webEx && webEx.StatusCode == StatusCodes.Status403Forbidden)
                    {
                        this.ShowNotification(Localized.Get("General.ForbiddenOperation"), Services.Models.NotificationSeverity.Error);
                    }
                    else
                    {
                        this.ShowNotification(Localized.Get("OperatorApp.CannotSuspendList"), Services.Models.NotificationSeverity.Warning);
                    }
                }
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.listExecuteCommand?.RaiseCanExecuteChanged();
            this.listDetailButtonCommand.RaiseCanExecuteChanged();
            this.selectOperationOnBayCommand?.RaiseCanExecuteChanged();
            this.suspendCommand?.RaiseCanExecuteChanged();
            this.showWaitingListCommand?.RaiseCanExecuteChanged();
            this.showExecutingListCommand?.RaiseCanExecuteChanged();
        }

        private bool CanChangeActiveList()
        {
            return !this.IsWaitingForResponse;
        }

        private bool CanExecuteList()
        {
            return
                this.areaId.HasValue
                &&
                this.SelectedLists.Any()
                &&
                !this.SelectedLists.Exists(x => x.ExecutionMode == ListExecutionMode.None)
                &&
                !this.ShowOnlyExecutingList;
        }

        private bool CanSelectOperationOnBay()
        {
            return this.sessionService.UserAccessLevel > UserAccessLevel.Movement;
        }

        private bool CanShowDetailCommand()
        {
            return this.SelectedLists.Count == 1;
        }

        private bool CanSuspendList()
        {
            return
                this.areaId.HasValue
                &&
                this.SelectedLists.Any()
                &&
                !this.SelectedLists.Exists(x => x.ExecutionMode == ListExecutionMode.None)
                &&
                this.ShowOnlyExecutingList;
        }

        private async Task ChangeActiveList(bool showExecuting)
        {
            this.ShowOnlyExecutingList = showExecuting;
            await this.LoadListsAsync();
        }

        /// <summary>
        /// Check if upcoming list are equal to the current one.
        /// </summary>
        /// <param name="newLists">Upcoming list read from EjLog</param>
        /// <returns>true if equal, false otherwise</returns>
        private bool CheckUpcomingItemLists(IEnumerable<ItemList> newLists)
        {
            var isEqual = true;

            IList<ItemListExecution> tmpLists = new List<ItemListExecution>();
            newLists.ForEach(l => tmpLists.Add(new ItemListExecution(l, this.machineId, this.priorityHighlighted)));

            isEqual = tmpLists.Count == this.lists.Count;

            if (tmpLists.Count == 0)
            {
                return false;
            }

            if (!isEqual)
            {
                return false;
            }

            var i = tmpLists.ToList();
            var j = this.Lists.ToList();

            isEqual = true;
            int jx = 0;
            while (jx < i.Count && isEqual)
            {
                var a = i[jx];
                var b = j[jx];
                jx++;

                // check equality about the class members
                if (a.Id != b.Id ||
                    a.IsDispatchable != b.IsDispatchable ||
                    a.Code != b.Code ||
                    a.Description != b.Description ||
                    a.ItemListRowsCount != b.ItemListRowsCount ||
                    a.ItemListType != b.ItemListType ||
                    a.Priority != b.Priority ||
                    a.ShipmentUnitCode != b.ShipmentUnitCode ||
                    a.ShipmentUnitDescription != b.ShipmentUnitDescription ||
                    a.Status != b.Status)
                {
                    isEqual = false;
                }
            }

            return isEqual;
        }

        //    try
        //    {
        //        var list = await this.itemListsWebService.GetByIdAsync(listId.Value);
        //        await this.ExecuteListAsync(new ItemListExecution(list, this.bayManager.Identity.Id));
        //    }
        //    catch
        //    {
        //        this.ShowNotification(
        //            string.Format(Resources.Localized.Get("OperatorApp.NoListWithIdWasFound"), listId.Value),
        //            Services.Models.NotificationSeverity.Error);
        //    }
        //}
        private async Task ExecuteListByBarcodeAsync(UserActionEventArgs e)
        {
            var listCode = e.GetListCode();
            if (listCode is null || listCode.Length == 0)
            {
                return;
            }

            if (this.IsCarrefour)
            {
                try
                {
                    this.SearchItem = listCode;

                    await this.FilterList(this.searchItem, this.tokenSource.Token);
                }
                catch (Exception)
                {
                }
            }
            else
            {
                try
                {
                    var list = await this.itemListsWebService.GetByNumAsync(listCode);

                    var BarcodeItemList = new List<ItemListExecution>();

                    BarcodeItemList.Add(new ItemListExecution(list.FirstOrDefault(), this.bayManager.Identity.Id, this.priorityHighlighted));

                    await this.ExecuteListAsync(BarcodeItemList);
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex);
                    this.ShowNotification(
                        string.Format(Resources.Localized.Get("OperatorApp.NoListWithIdWasFound"), listCode),
                        Services.Models.NotificationSeverity.Error);
                }
            }
        }

        private async Task FilterList(string searchItem, CancellationToken cancellationToken)
        {
            try
            {
                this.IsWaitingForResponse = true;

                if (string.IsNullOrEmpty(searchItem))
                {
                    this.IsWaitingForResponse = false;
                    await this.RefreshListsAsync();
                }
                else
                {
                    var bay = this.MachineService.Bay;
                    var fullLists = await this.areasWebService.GetItemListsAsync(this.areaId.Value, this.machineId, bay.Id, false, this.authenticationService.UserName);

                    var ItemExecutionList = new List<ItemListExecution>();
                    fullLists.ForEach(x => ItemExecutionList.Add(new ItemListExecution(x, this.machineId, this.priorityHighlighted)));

                    this.lists.Clear();

                    foreach (var item in ItemExecutionList)
                    {
                        if (item.Code.ToLower().Contains(searchItem.ToLower()))
                        {
                            this.lists.Add(item);
                        }
                    }

                    this.RaisePropertyChanged(nameof(this.Lists));
                    this.SelectedList = null;
                    this.SelectedLists.Clear();
                    this.SelectLoadingUnit();
                }
            }
            catch (TaskCanceledException)
            {
                // normal situation
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.SearchItem = string.Empty;
                this.IsWaitingForResponse = false;
            }
            finally
            {
            }
        }

        //private async Task ExecuteListByBarcodeAsync(UserActionEventArgs e)
        //{
        //    var listId = e.GetListId();
        //    if (!listId.HasValue)
        //    {
        //        return;
        //    }
        private async Task FilterListsByBarcodeAsync(UserActionEventArgs e)
        {
            var listCode = e.GetListCode();
            if (listCode is null || listCode.Length == 0)
            {
                this.ShowNotification(
                   string.Format(Resources.Localized.Get("OperatorApp.BarcodeDoesNotContainTheListId"), e.Code),
                   Services.Models.NotificationSeverity.Warning);

                return;
            }

            try
            {
                var list = await this.itemListsWebService.GetByNumAsync(listCode);

                this.ShowDetails(new ItemListExecution(list.FirstOrDefault(), this.bayManager.Identity.Id, this.priorityHighlighted));
            }
            catch
            {
                this.ShowNotification(
                  string.Format(Resources.Localized.Get("OperatorApp.BarcodeDoesNotContainTheListId"), e.Code),
                  Services.Models.NotificationSeverity.Error);
            }
        }

        private async Task LoadListsAsync()
        {
            if (!this.areaId.HasValue
                ||
                this.IsWaitingForResponse)
            {
                return;
            }

            try
            {
                this.IsWaitingForResponse = true;

                var bay = this.MachineService.Bay;
                var lastItemListId = this.selectedList?.Id;
                var newLists = await this.areasWebService.GetItemListsAsync(this.areaId.Value, this.machineId, bay.Id, this.ShowOnlyExecutingList, this.authenticationService.UserName);

                // check upcoming lists retrieved from EjLog
                if (this.CheckUpcomingItemLists(newLists))
                {
                    this.IsWaitingForResponse = false;
                    return;
                }

                this.lists.Clear();

                if (this.isWaitingListLocal is true)
                {
                    foreach (var newList in newLists.Where(x => x.Machines?.Any(m => m.Id == this.machineId) == true).OrderBy(l => l.IsDispatchable))
                    {
                        this.lists.Add(new ItemListExecution(newList, this.machineId, this.priorityHighlighted));
                    }
                }
                else
                {
                    newLists.ForEach(l => this.lists.Add(new ItemListExecution(l, this.machineId, this.priorityHighlighted)));
                }

                if (this.lists.Count > 0)
                {
                    this.IsShipmentDayVisible = this.lists.Any(i => i.ShipmentUnitCode != null);

                    this.lists.Sort((x, y) =>
                    {
                        var compare = x.ExecutionMode.CompareTo(y.ExecutionMode);
                        return compare != 0 ? compare : x.Code.CompareTo(y.Code);
                    });
                }

                this.RaisePropertyChanged(nameof(this.Lists));
                this.RaisePropertyChanged(nameof(this.IsShipmentDayVisible));

                this.RaisePropertyChanged(nameof(this.selectedLists));
                this.SetCurrentIndex(lastItemListId);

                this.SelectLoadingUnit();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.lists?.Clear();
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task RefreshListsAsync()
        {
            while (this.IsVisible)
            {
                await this.LoadListsAsync();
                await Task.Delay(PollIntervalMilliseconds);
            }
        }

        private void SelectLoadingUnit()
        {
            this.SelectedList = null;

            this.RaiseCanExecuteChanged();
        }

        private void SetCurrentIndex(int? itemListId)
        {
            if (itemListId.HasValue
                &&
                this.lists.FirstOrDefault(l => l.Id == itemListId.Value) is ItemListExecution itemListFound)
            {
                this.currentItemIndex = this.lists.IndexOf(itemListFound);
            }
            else
            {
                this.currentItemIndex = 0;
            }
        }

        private void ShowDetails(ItemListExecution list)
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.WaitingLists.DETAIL,
                    list,
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

        private void ShowOperationOnBay()
        {
            try
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.Others.OPERATIONONBAY,
                    null,
                    trackCurrentView: true);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task TriggerSearchAsync()
        {
            this.tokenSource?.Cancel(false);

            this.tokenSource = new CancellationTokenSource();

            try
            {
                const int callDelayMilliseconds = 500;

                await Task.Delay(callDelayMilliseconds, this.tokenSource.Token);
                await this.FilterList(this.searchItem, this.tokenSource.Token);
            }
            catch (TaskCanceledException)
            {
            }
        }

        #endregion
    }
}
