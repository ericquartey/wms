﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class BaseLoadingUnitViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineLoadingUnitsWebService loadingUnitsWebService;

        private DelegateCommand changeModeListCommand;

        private DelegateCommand changeModeLoadingUnitCommand;

        private IEnumerable<TrayControlCompartment> compartments;

        private int currentItemCompartmentIndex;

        private int currentItemIndex;

        private int? currentLoadingUnitId;

        private bool isBusyConfirmingOperation;

        private bool isBusyConfirmingRecallOperation;

        private bool isListModeEnabled;

        private bool isNewOperationAvailable;

        private DelegateCommand itemCompartmentDownCommand;

        private DelegateCommand itemCompartmentUpCommand;

        private DelegateCommand itemDownCommand;

        private IEnumerable<CompartmentDetails> items;

        private IEnumerable<CompartmentDetails> itemsCompartments;

        private DelegateCommand itemUpCommand;

        private double loadingUnitDepth;

        private double loadingUnitWidth;

        private SubscriptionToken missionOperationToken;

        private string recallLoadingUnitInfo = OperatorApp.RecallDrawer;

        private TrayControlCompartment selectedCompartment;

        private CompartmentDetails selectedItem;

        private CompartmentDetails selectedItemCompartment;

        #endregion

        #region Constructors

        public BaseLoadingUnitViewModel(
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator)
            : base(PresentationMode.Operator)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.loadingUnitsWebService = loadingUnitsWebService ?? throw new ArgumentNullException(nameof(loadingUnitsWebService));
            this.MissionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.CompartmentColoringFunction = (compartment, selectedCompartment) => this.itemsCompartments?.Any(ic => ic.Id == compartment.Id && ic.ItemId != null) == true ? "#444444" : "#222222";
        }

        #endregion

        #region Properties

        public ICommand ChangeModeListCommand =>
            this.changeModeListCommand
            ??
            (this.changeModeListCommand = new DelegateCommand(this.ChangeMode, this.CanSwitchToListMode));

        public ICommand ChangeModeLoadingUnitCommand =>
               this.changeModeLoadingUnitCommand
               ??
               (this.changeModeLoadingUnitCommand = new DelegateCommand(this.ChangeMode, this.CanSwitchToLoadingUnitMode));

        public Func<IDrawableCompartment, IDrawableCompartment, string> CompartmentColoringFunction { get; }

        public IEnumerable<TrayControlCompartment> Compartments
        {
            get => this.compartments;
            set => this.SetProperty(ref this.compartments, value, this.RaiseCanExecuteChanged);
        }

        public string ConfirmOperationInfo => this.isNewOperationAvailable ? OperatorApp.ConfirmAndNewOperationsAvailable : OperatorApp.Confirm;

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsBaySideBack => this.MachineService.Bay.Side is WarehouseSide.Back;

        public bool IsBusyConfirmingOperation
        {
            get => this.isBusyConfirmingOperation;
            set => this.SetProperty(ref this.isBusyConfirmingOperation, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBusyConfirmingRecallOperation
        {
            get => this.isBusyConfirmingRecallOperation;
            set => this.SetProperty(ref this.isBusyConfirmingRecallOperation, value, this.RaiseCanExecuteChanged);
        }

        public bool IsListModeEnabled
        {
            get => this.isListModeEnabled;
            set => this.SetProperty(ref this.isListModeEnabled, value, this.RaiseCanExecuteChanged);
        }

        public bool IsNewLoadingUnit => this.currentLoadingUnitId != this.LoadingUnit?.Id;

        public bool IsNewOperationAvailable
        {
            get => this.isNewOperationAvailable;
            set
            {
                if (this.SetProperty(ref this.isNewOperationAvailable, value))
                {
                    this.RecallLoadingUnitInfo = value
                        ? OperatorApp.NewOperationsAvailable
                        : OperatorApp.RecallDrawer;
                }
            }
        }

        public bool IsWmsEnabledAndHealthy =>
            this.IsWmsHealthy
            &&
            ConfigurationManager.AppSettings.GetWmsDataServiceEnabled();

        public override bool IsWmsHealthy
        {
            get => base.IsWmsHealthy;
            set
            {
                if (value != base.IsWmsHealthy)
                {
                    base.IsWmsHealthy = value;
                    this.RaisePropertyChanged(nameof(this.IsWmsEnabledAndHealthy));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand ItemCompartmentDownCommand =>
              this.itemCompartmentDownCommand
              ??
              (this.itemCompartmentDownCommand = new DelegateCommand(() => this.ChangeSelectedItemCompartment(false), this.CanSelectNextItemCompartment));

        public ICommand ItemCompartmentUpCommand =>
              this.itemCompartmentUpCommand
              ??
              (this.itemCompartmentUpCommand = new DelegateCommand(() => this.ChangeSelectedItemCompartment(true), this.CanSelectPreviousItemCompartment));

        public ICommand ItemDownCommand =>
            this.itemDownCommand
            ??
            (this.itemDownCommand = new DelegateCommand(() => this.ChangeSelectedItem(false), this.CanSelectNextItem));

        public IEnumerable<CompartmentDetails> Items
        {
            get => this.items;
            set => this.SetProperty(ref this.items, value);
        }

        public IEnumerable<CompartmentDetails> ItemsCompartments
        {
            get => this.itemsCompartments;
            set => this.SetProperty(ref this.itemsCompartments, value, this.RaiseCanExecuteChanged);
        }

        public ICommand ItemUpCommand =>
            this.itemUpCommand
            ??
            (this.itemUpCommand = new DelegateCommand(() => this.ChangeSelectedItem(true), this.CanSelectPreviousItem));

        public LoadingUnit LoadingUnit { get; set; }

        public double LoadingUnitDepth
        {
            get => this.loadingUnitDepth;
            set => this.SetProperty(ref this.loadingUnitDepth, value);
        }

        public double LoadingUnitWidth
        {
            get => this.loadingUnitWidth;
            set => this.SetProperty(ref this.loadingUnitWidth, value);
        }

        public string RecallLoadingUnitInfo
        {
            get => this.recallLoadingUnitInfo;
            set => this.SetProperty(ref this.recallLoadingUnitInfo, value);
        }

        public TrayControlCompartment SelectedCompartment
        {
            get => this.selectedCompartment;
            set
            {
                if (this.SetProperty(ref this.selectedCompartment, value, this.SetItemsAndCompartment))
                {
                    if (this.selectedCompartment is null)
                    {
                        this.SelectedItemCompartment = null;
                    }
                    else if (this.SelectedItemCompartment is null || this.SelectedItemCompartment.Id != this.selectedCompartment.Id)
                    {
                        var newSelectedItemCompartment = this.itemsCompartments?.FirstOrDefault(c => c.Id == this.selectedCompartment.Id);
                        this.currentItemCompartmentIndex = this.itemsCompartments.ToList().IndexOf(newSelectedItemCompartment);
                        this.SelectedItemCompartment = newSelectedItemCompartment;
                    }
                }
            }
        }

        public CompartmentDetails SelectedItem
        {
            get => this.selectedItem;
            set => this.SetProperty(ref this.selectedItem, value, this.SetSelectedItemCompartment);
        }

        public CompartmentDetails SelectedItemCompartment
        {
            get => this.selectedItemCompartment;
            set => this.SetProperty(ref this.selectedItemCompartment, value, this.SetSelectedItemAndCompartment);
        }

        protected IMissionOperationsService MissionOperationsService { get; }

        #endregion

        #region Methods

        public void ChangeSelectedItem(bool selectPrevious)
        {
            if (this.items is null)
            {
                return;
            }

            if (this.items.Any())
            {
                var newIndex = selectPrevious ? this.currentItemIndex - 1 : this.currentItemIndex + 1;

                this.currentItemIndex = Math.Max(0, Math.Min(newIndex, this.items.Count() - 1));
            }

            this.SetSelectedItem();
        }

        public void ChangeSelectedItemCompartment(bool selectPrevious)
        {
            if (this.itemsCompartments is null)
            {
                return;
            }

            if (this.itemsCompartments.Any())
            {
                var newIndex = selectPrevious ? this.currentItemCompartmentIndex - 1 : this.currentItemCompartmentIndex + 1;

                this.currentItemCompartmentIndex = Math.Max(0, Math.Min(newIndex, this.itemsCompartments.Count() - 1));
            }

            this.SelectItemCompartment();
        }

        public override void Disappear()
        {
            base.Disappear();

            if (this.missionOperationToken != null)
            {
                this.EventAggregator.GetEvent<PubSubEvent<AssignedMissionChangedEventArgs>>().Unsubscribe(this.missionOperationToken);
                this.missionOperationToken?.Dispose();
                this.missionOperationToken = null;
            }
        }

        public async Task LoadCompartmentsAsync()
        {
            if (!this.IsWmsEnabledAndHealthy)
            {
                return;
            }

            try
            {
                var wmsLoadingUnit = await this.loadingUnitsWebService.GetWmsDetailsByIdAsync(this.LoadingUnit.Id);
                this.LoadingUnitWidth = wmsLoadingUnit.Width;
                this.LoadingUnitDepth = wmsLoadingUnit.Depth;

                this.ItemsCompartments = await this.loadingUnitsWebService.GetCompartmentsAsync(this.LoadingUnit.Id);
                this.Compartments = MapCompartments(this.ItemsCompartments);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                // compartment details will not be shown
                this.ItemsCompartments = null;
                this.Compartments = null;
            }
        }

        public async override Task OnAppearedAsync()
        {
            if (this.Data is int loadingUnitId)
            {
                this.LoadingUnit = this.MachineService.Loadunits.Single(l => l.Id == loadingUnitId);
                this.RaisePropertyChanged(nameof(this.LoadingUnit));
            }
            else
            {
                throw new Exception($"The '{nameof(this.Data)}' property of the '{this.GetType().Name}' view model shall contain a valid loading unit identifier.");
            }

            this.missionOperationToken = this.eventAggregator
                .GetEvent<PubSubEvent<MissionChangedEventArgs>>()
                .Subscribe(this.OnMissionChanged);

            this.OnMissionChanged(null);

            var canReset =
                this.IsNewLoadingUnit
                &&
                !this.isNewOperationAvailable
                &&
                !this.IsBusyConfirmingOperation
                &&
                !this.IsBusyConfirmingRecallOperation;

            if (canReset)
            {
                this.Reset();
            }

            await base.OnAppearedAsync();
        }

        public virtual void RaisePropertyChanged()
        {
            this.RaisePropertyChanged(nameof(this.SelectedCompartment));
            this.RaisePropertyChanged(nameof(this.LoadingUnit));
            this.RaisePropertyChanged(nameof(this.SelectedItem));
            this.RaisePropertyChanged(nameof(this.SelectedItemCompartment));
        }

        protected override async Task OnDataRefreshAsync()
        {
            await this.LoadCompartmentsAsync();

            var lastCompartmentId = this.SelectedItemCompartment?.Id;
            if (lastCompartmentId != null)
            {
                this.SelectedCompartment = this.Compartments.FirstOrDefault(ic => ic.Id == lastCompartmentId);
            }

            var lastItemId = this.SelectedItemCompartment?.ItemId;
            if (lastItemId != null)
            {
                this.SelectedItem = this.Items.FirstOrDefault(ic => ic.ItemId == lastItemId);
            }

            await base.OnDataRefreshAsync();
        }

        protected async override Task OnMachineModeChangedAsync(MachineModeChangedEventArgs e)
        {
            await base.OnMachineModeChangedAsync(e);

            this.RaiseCanExecuteChanged();
        }

        protected virtual void OnSelectedCompartmentChanged()
        {
            // do nothing
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.changeModeLoadingUnitCommand?.RaiseCanExecuteChanged();
            this.changeModeListCommand?.RaiseCanExecuteChanged();
            this.itemCompartmentDownCommand?.RaiseCanExecuteChanged();
            this.itemCompartmentUpCommand?.RaiseCanExecuteChanged();
            this.itemDownCommand?.RaiseCanExecuteChanged();
            this.itemUpCommand?.RaiseCanExecuteChanged();

            base.RaiseCanExecuteChanged();
        }

        protected void Reset()
        {
            this.currentItemCompartmentIndex = 0;
            this.currentItemIndex = 0;
            this.IsListModeEnabled = false;
            this.SelectedItemCompartment = null;
            this.SelectedCompartment = null;
            this.SelectedItem = null;
            this.currentLoadingUnitId = this.LoadingUnit?.Id;
        }

        private static IEnumerable<TrayControlCompartment> MapCompartments(IEnumerable<CompartmentDetails> compartmentsFromMission)
        {
            return compartmentsFromMission
                .GroupBy(c => c.Id)
                .Select(c => new TrayControlCompartment
                {
                    Depth = c.First().Depth,
                    Id = c.First().Id,
                    Width = c.First().Width,
                    XPosition = c.First().XPosition,
                    YPosition = c.First().YPosition,
                });
        }

        private bool CanSelectNextItem() =>
            this.items != null
            &&
            this.currentItemIndex < this.items.Count() - 1;

        private bool CanSelectNextItemCompartment() =>
            this.itemsCompartments != null
            &&
            this.currentItemCompartmentIndex < this.itemsCompartments.Count() - 1;

        private bool CanSelectPreviousItem() => this.currentItemIndex > 0;

        private bool CanSelectPreviousItemCompartment() => this.currentItemCompartmentIndex > 0;

        private bool CanSwitchToListMode() =>
            !this.isListModeEnabled
            &&
            !this.IsWaitingForResponse
            &&
            !this.IsBusyConfirmingOperation
            &&
            !this.IsBusyConfirmingRecallOperation
            &&
            this.itemsCompartments != null
            &&
            this.itemsCompartments.Any();

        private bool CanSwitchToLoadingUnitMode() =>
            this.isListModeEnabled
            &&
            !this.IsWaitingForResponse
            &&
            !this.IsBusyConfirmingOperation
            &&
            !this.IsBusyConfirmingRecallOperation
            &&
            this.itemsCompartments != null
            &&
            this.itemsCompartments.Any();

        private void ChangeMode()
        {
            if (!this.isListModeEnabled
                &&
                this.selectedItemCompartment is null)
            {
                this.SelectedItemCompartment = this.itemsCompartments?.FirstOrDefault();
            }

            this.IsListModeEnabled = !this.IsListModeEnabled;
        }

        private void OnMissionChanged(MissionChangedEventArgs e)
        {
            this.IsNewOperationAvailable =
                ConfigurationManager.AppSettings.GetWmsDataServiceEnabled()
                &&
                this.MissionOperationsService.ActiveWmsMission != null
                &&
                (
                    this.MissionOperationsService.ActiveWmsOperation.Type != MissionOperationType.LoadingUnitCheck
                    ||
                    this.MissionOperationsService.ActiveWmsMission.Operations.Any(o =>
                        o.Status != MissionOperationStatus.Completed
                        &&
                        o.Id != this.MissionOperationsService.ActiveWmsOperation?.Id));
        }

        private void SelectItemCompartment()
        {
            if (this.itemsCompartments.Any())
            {
                this.SelectedItemCompartment = this.itemsCompartments.ElementAt(this.currentItemCompartmentIndex);
            }
            else
            {
                this.SelectedItemCompartment = null;
            }

            this.RaiseCanExecuteChanged();
        }

        private void SetItems()
        {
            this.Items = this.ItemsCompartments.Where(ic => ic.Id == this.selectedCompartment.Id && ic.ItemId.HasValue);
        }

        private void SetItemsAndCompartment()
        {
            if (this.selectedCompartment is null)
            {
                this.Items = null;
                this.selectedItem = null;
                return;
            }

            this.SetItems();

            if (this.Items.Any() == true)
            {
                this.SelectedItem = this.Items.First();
            }

            this.currentItemIndex = 0;

            this.RaiseCanExecuteChanged();

            this.OnSelectedCompartmentChanged();
        }

        private void SetSelectedItem()
        {
            if (this.items?.Any() == true)
            {
                this.SelectedItem = this.items.ElementAt(this.currentItemIndex);
            }
            else
            {
                this.SelectedItem = null;
            }

            this.RaiseCanExecuteChanged();
        }

        private void SetSelectedItemAndCompartment()
        {
            if (this.selectedItemCompartment is null)
            {
                this.RaiseCanExecuteChanged();
                return;
            }

            this.SelectedCompartment = this.Compartments.FirstOrDefault(c => c.Id == this.selectedItemCompartment.Id);
            this.SetItems();
            this.RaisePropertyChanged();
            if (this.items?.Any() == true)
            {
                if (this.items.FirstOrDefault(ic => ic.ItemId == this.selectedItemCompartment.ItemId) is CompartmentDetails newSelectedItem)
                {
                    this.currentItemIndex = this.items.ToList().IndexOf(newSelectedItem);
                    this.selectedItem = newSelectedItem;
                    this.RaisePropertyChanged();
                }
            }

            this.RaiseCanExecuteChanged();
        }

        private void SetSelectedItemCompartment()
        {
            if (this.selectedItem is null)
            {
                this.SelectedItemCompartment = null;
                return;
            }

            if (this.itemsCompartments.FirstOrDefault(ic => ic.Id == this.selectedCompartment.Id
                                                            &&
                                                            ic.ItemId == this.selectedItem.ItemId) is CompartmentDetails newSelectedItemCompartment)
            {
                this.currentItemCompartmentIndex = this.itemsCompartments.ToList().IndexOf(newSelectedItemCompartment);
                this.selectedItemCompartment = newSelectedItemCompartment;
                this.RaisePropertyChanged();
            }

            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
