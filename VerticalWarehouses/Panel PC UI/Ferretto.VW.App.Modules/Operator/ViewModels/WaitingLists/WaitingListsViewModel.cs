using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Microsoft.AspNetCore.Http;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class WaitingListsViewModel : BaseOperatorViewModel, IOperationalContextViewModel
    {
        #region Fields

        private const int PollIntervalMilliseconds = 5000;

        private readonly IMachineAreasWebService areasWebService;

        private readonly IAuthenticationService authenticationService;

        private readonly IBayManager bayManager;

        private readonly IMachineIdentityWebService identityService;

        private readonly IMachineItemListsWebService itemListsWebService;

        private readonly IList<ItemListExecution> lists = new List<ItemListExecution>();

        private int? areaId;

        private int currentItemIndex;

        private DelegateCommand listDetailButtonCommand;

        private DelegateCommand listExecuteCommand;

        private int machineId;

        private ItemListExecution selectedList;

        #endregion

        #region Constructors

        public WaitingListsViewModel(
            IMachineIdentityWebService identityService,
            IMachineItemListsWebService itemListsWebService,
            IMachineAreasWebService areasWebService,
            IBayManager bayManager,
            IAuthenticationService authenticationService)
            : base(PresentationMode.Operator)
        {
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.itemListsWebService = itemListsWebService ?? throw new ArgumentNullException(nameof(itemListsWebService));
            this.areasWebService = areasWebService ?? throw new ArgumentNullException(nameof(areasWebService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        #endregion

        #region Properties

        public string ActiveContextName => OperationalContext.ListSearch.ToString();

        public override bool KeepAlive => true;

        public ICommand ListDetailButtonCommand =>
            this.listDetailButtonCommand
            ??
            (this.listDetailButtonCommand = new DelegateCommand(
                () => this.ShowDetails(this.selectedList),
                this.CanShowDetailCommand));

        public ICommand ListExecuteCommand =>
            this.listExecuteCommand
            ??
            (this.listExecuteCommand = new DelegateCommand(
                async () => await this.ExecuteListAsync(this.selectedList),
                this.CanExecuteList));

        public IList<ItemListExecution> Lists => new List<ItemListExecution>(this.lists);

        public ItemListExecution SelectedList
        {
            get => this.selectedList;
            set => this.SetProperty(ref this.selectedList, value, this.RaiseCanExecuteChanged);
        }

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

        public async Task ExecuteListAsync(ItemListExecution itemList)
        {
            try
            {
                if (!this.areaId.HasValue)
                {
                    return;
                }

                var bay = await this.bayManager.GetBayAsync();
                await this.itemListsWebService.ExecuteAsync(itemList.Id, this.areaId.Value, bay.Id, this.authenticationService.UserName);
                await this.LoadListsAsync();
                this.ShowNotification(
                    string.Format(Resources.Localized.Get("OperatorApp.ExecutionOfListAccepted"), itemList.Code),
                    Services.Models.NotificationSeverity.Success);
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

            await this.LoadListsAsync();

            await this.RefreshListsAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.listExecuteCommand?.RaiseCanExecuteChanged();
            this.listDetailButtonCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteList()
        {
            return
                this.areaId.HasValue
                &&
                this.SelectedList != null
                &&
                this.SelectedList.ExecutionMode != ListExecutionMode.None;
        }

        private bool CanShowDetailCommand()
        {
            return this.SelectedList != null;
        }

        //private async Task ExecuteListByBarcodeAsync(UserActionEventArgs e)
        //{
        //    var listId = e.GetListId();
        //    if (!listId.HasValue)
        //    {
        //        return;
        //    }

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

            try
            {
                var list = await this.itemListsWebService.GetByNumAsync(listCode);
                await this.ExecuteListAsync(new ItemListExecution(list.FirstOrDefault(), this.bayManager.Identity.Id));
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex);
                this.ShowNotification(
                    string.Format(Resources.Localized.Get("OperatorApp.NoListWithIdWasFound"), listCode),
                    Services.Models.NotificationSeverity.Error);
            }
        }

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

                this.ShowDetails(new ItemListExecution(list.FirstOrDefault(), this.bayManager.Identity.Id));
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

                var lastItemListId = this.selectedList?.Id;
                var newLists = await this.areasWebService.GetItemListsAsync(this.areaId.Value);

                this.lists.Clear();
                newLists.ForEach(l => this.lists.Add(new ItemListExecution(l, this.machineId)));

                this.RaisePropertyChanged(nameof(this.Lists));

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
            if (this.lists.Any())
            {
                this.SelectedList = this.lists.ElementAt(this.currentItemIndex);
            }
            else
            {
                this.SelectedList = null;
            }

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

        #endregion
    }
}
