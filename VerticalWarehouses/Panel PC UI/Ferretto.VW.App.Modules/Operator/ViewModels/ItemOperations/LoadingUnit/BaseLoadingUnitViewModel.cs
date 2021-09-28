using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.LaserPointer;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class BaseLoadingUnitViewModel : BaseOperatorViewModel
    {
        #region Fields

        public readonly IEventAggregator eventAggregator;

        private readonly IBayManager bayManager;

        private readonly ILaserPointerDriver laserPointerDriver;

        private readonly IMachineLoadingUnitsWebService loadingUnitsWebService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly ISessionService sessionService;

        private DelegateCommand changeModeListCommand;

        private DelegateCommand changeModeLoadingUnitCommand;

        private IEnumerable<TrayControlCompartment> compartments;

        private int currentItemCompartmentIndex;

        private int currentItemIndex;

        private int? currentLoadingUnitId;

        private bool isBoxEnabled;

        private bool isBusyConfirmingOperation;

        private bool isBusyConfirmingRecallOperation;

        private bool isLaserEnabled;

        private bool isLaserOffEnabled;

        private bool isLaserOnEnabled;

        private bool isListModeEnabled;

        private bool isNewOperationAvailable;

        private bool isUpperPosition;

        private DelegateCommand itemCompartmentDownCommand;

        private DelegateCommand itemCompartmentUpCommand;

        private DelegateCommand itemDownCommand;

        private bool itemLotVisibility;

        private IEnumerable<CompartmentDetails> items;

        private IEnumerable<CompartmentDetails> itemsCompartments;

        private bool itemSerialNumberVisibility;

        private DelegateCommand itemUpCommand;

        private DelegateCommand laserOffCommand;

        private DelegateCommand laserOnCommand;

        private double loadingUnitDepth;

        private double loadingUnitWidth;

        private SubscriptionToken missionOperationToken;

        private string recallLoadingUnitInfo = Localized.Get("OperatorApp.RecallDrawer");

        private TrayControlCompartment selectedCompartment;

        private CompartmentDetails selectedItem;

        private CompartmentDetails selectedItemCompartment;

        #endregion

        #region Constructors

        public BaseLoadingUnitViewModel(
            IMachineIdentityWebService machineIdentityWebService,
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            ILaserPointerDriver laserPointerDriver,
            ISessionService sessionService,
            IWmsDataProvider wmsDataProvider)
            : base(PresentationMode.Operator)
        {
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.WmsDataProvider = wmsDataProvider ?? throw new ArgumentNullException(nameof(wmsDataProvider));
            this.loadingUnitsWebService = loadingUnitsWebService ?? throw new ArgumentNullException(nameof(loadingUnitsWebService));
            this.MissionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.CompartmentColoringFunction = (compartment, selectedCompartment) => this.itemsCompartments?.Any(ic => ic.Id == compartment.Id && ic.ItemId != null) == true ? "#444444" : "#222222";
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.laserPointerDriver = laserPointerDriver ?? throw new ArgumentNullException(nameof(laserPointerDriver));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
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

        public string ConfirmOperationInfo => this.isNewOperationAvailable ? Localized.Get("OperatorApp.ConfirmAndNewOperationsAvailable") : Localized.Get("OperatorApp.Confirm");

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsBaySideBack => this.MachineService.Bay.Side is WarehouseSide.Back;

        public bool IsBoxEnabled
        {
            get => this.isBoxEnabled;
            set => this.SetProperty(ref this.isBoxEnabled, value, this.RaiseCanExecuteChanged);
        }

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

        public bool IsLaserOffEnabled
        {
            get => this.isLaserOffEnabled;
            set => this.SetProperty(ref this.isLaserOffEnabled, value, this.RaiseCanExecuteChanged);
        }

        public bool IsLaserOnEnabled
        {
            get => this.isLaserOnEnabled;
            set => this.SetProperty(ref this.isLaserOnEnabled, value, this.RaiseCanExecuteChanged);
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
                this.SetProperty(ref this.isNewOperationAvailable, value);

                var result = value
                    ? Localized.Get("OperatorApp.NewOperationsAvailable")
                    : Localized.Get("OperatorApp.RecallDrawer");

                if (result != this.recallLoadingUnitInfo)
                {
                    this.recallLoadingUnitInfo = result;
                }
            }
        }

        public bool IsWmsEnabledAndHealthy =>
            this.IsWmsHealthy
            &&
            this.WmsDataProvider.IsEnabled;

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

        public bool ItemLotVisibility
        {
            get => this.itemLotVisibility;
            set => this.SetProperty(ref this.itemLotVisibility, value);
        }

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

        public bool ItemSerialNumberVisibility
        {
            get => this.itemSerialNumberVisibility;
            set => this.SetProperty(ref this.itemSerialNumberVisibility, value);
        }

        public ICommand ItemUpCommand =>
            this.itemUpCommand
            ??
            (this.itemUpCommand = new DelegateCommand(() => this.ChangeSelectedItem(true), this.CanSelectPreviousItem));

        public ICommand LaserOffCommand =>
            this.laserOffCommand
            ??
            (this.laserOffCommand = new DelegateCommand(async () => await this.LaserOffAsync(), this.CanLaser));

        public ICommand LaserOnCommand =>
                    this.laserOnCommand
            ??
            (this.laserOnCommand = new DelegateCommand(async () => await this.LaserOnAsync(), this.CanLaser));

        public LoadingUnit LoadingUnit { get; set; }

        public double LoadingUnitDepth
        {
            get => this.loadingUnitDepth;
            set => this.SetProperty(ref this.loadingUnitDepth, value);
        }

        public IMachineLoadingUnitsWebService LoadingUnitsWebService => this.loadingUnitsWebService;

        public double LoadingUnitWidth
        {
            get => this.loadingUnitWidth;
            set => this.SetProperty(ref this.loadingUnitWidth, value);
        }

        public string RecallLoadingUnitInfo
        {
            get => this.recallLoadingUnitInfo;
            set => this.SetProperty(ref this.recallLoadingUnitInfo, value, this.RaiseCanExecuteChanged);
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

        protected IWmsDataProvider WmsDataProvider { get; }

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

        public async Task LaserOffAsync()
        {
            this.Logger.Info($"Switch off laser pointer");
            await this.laserPointerDriver.EnabledAsync(false, false);
            this.IsLaserOnEnabled = true;
            this.IsLaserOffEnabled = false;
        }

        public async Task LaserOnAsync()
        {
            var point = this.laserPointerDriver.CalculateLaserPoint(
                this.loadingUnitWidth,
                this.loadingUnitDepth,
                this.selectedCompartment.Width.Value,
                this.selectedCompartment.Depth.Value,
                this.selectedCompartment.XPosition.Value,
                this.selectedCompartment.YPosition.Value,
                this.LoadingUnit.LaserOffset,
                this.isUpperPosition,
                this.IsBaySideBack ? WarehouseSide.Back : WarehouseSide.Front
                );

            this.Logger.Info($"Move and switch on laser pointer to compartment {this.selectedCompartment.Id}; " +
                $"luW {this.loadingUnitWidth}; " +
                $"luD {this.loadingUnitDepth}; " +
                $"cw {this.selectedCompartment.Width.Value}; " +
                $"cd {this.selectedCompartment.Depth.Value}; " +
                $"cx {this.selectedCompartment.XPosition.Value}; " +
                $"cy {this.selectedCompartment.YPosition.Value}; " +
                $"z {this.LoadingUnit.LaserOffset}; " +
                $"up {this.isUpperPosition}; " +
                $"back {this.IsBaySideBack}");
            await this.laserPointerDriver.MoveAndSwitchOnAsync(point, false);
            this.IsLaserOnEnabled = false;
            this.IsLaserOffEnabled = true;
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

                var itemsCompartments = await this.loadingUnitsWebService.GetCompartmentsAsync(this.LoadingUnit.Id);
                this.ItemsCompartments = itemsCompartments?.Where(ic => !(ic.ItemId is null));
                this.Compartments = MapCompartments(itemsCompartments);
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
            this.ItemSerialNumberVisibility = false;
            this.ItemLotVisibility = false;

            //string value = System.Configuration.ConfigurationManager.AppSettings["Box"];

            //this.IsBoxEnabled = value.ToLower() == "true" ? true : false;

            this.IsBoxEnabled = await this.machineIdentityWebService.GetBoxEnableAsync();

            var accessories = await this.bayManager.GetBayAccessoriesAsync();
            this.isLaserEnabled = accessories?.LaserPointer?.IsEnabledNew ?? false;
            this.IsLaserOnEnabled = this.isLaserEnabled && this.sessionService.UserAccessLevel != UserAccessLevel.Operator;
            this.IsLaserOffEnabled = false;

            if (this.Data is int loadingUnitId)
            {
                this.LoadingUnit = this.MachineService.Loadunits.Single(l => l.Id == loadingUnitId);
                this.RaisePropertyChanged(nameof(this.LoadingUnit));
            }
            else
            {
                throw new Exception(string.Format(Resources.Localized.Get("General.LoadingUnitViewModelPropertyIdentifier"), nameof(this.Data), this.GetType().Name));
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

            this.NoteEnabled = true;
            this.RaisePropertyChanged(nameof(this.NoteEnabled));
        }

        public virtual void RaisePropertyChanged()
        {
            this.RaisePropertyChanged(nameof(this.SelectedCompartment));
            this.RaisePropertyChanged(nameof(this.LoadingUnit));
            this.RaisePropertyChanged(nameof(this.SelectedItem));
            this.RaisePropertyChanged(nameof(this.SelectedItemCompartment));
            this.RaisePropertyChanged(nameof(this.ItemSerialNumberVisibility));
            this.RaisePropertyChanged(nameof(this.ItemLotVisibility));
        }

        protected override async Task OnDataRefreshAsync()
        {
            await this.LoadCompartmentsAsync();

            var lastCompartmentId = this.SelectedItemCompartment?.Id;
            try
            {
                if (lastCompartmentId != null)
                {
                    this.SelectedCompartment = this.Compartments.FirstOrDefault(ic => ic.Id == lastCompartmentId);
                }
                else if (this.Compartments?.Count() == 1)
                {
                    this.SelectedCompartment = this.Compartments.First();
                }
            }
            catch (Exception)
            {
                // do nothing
            }

            var lastItemId = this.SelectedItemCompartment?.ItemId;
            try
            {
                if (lastItemId != null)
                {
                    this.SelectedItem = this.Items.FirstOrDefault(ic => ic.ItemId == lastItemId);
                }
                else if (this.Items?.Count() == 1)
                {
                    this.SelectedItem = this.Items.First();
                }
            }
            catch (Exception)
            {
                // do nothing
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
            this.laserOnCommand?.RaiseCanExecuteChanged();
            this.laserOffCommand?.RaiseCanExecuteChanged();

            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.RecallLoadingUnitInfo));
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
                    Barcode = c.First().Barcode,
                });
        }

        private bool CanLaser() =>
            this.isLaserEnabled
            && this.selectedCompartment != null
            && this.selectedCompartment.Width.HasValue
            && this.selectedCompartment.Depth.HasValue
            && this.selectedCompartment.XPosition.HasValue
            && this.selectedCompartment.YPosition.HasValue;

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
            this.IsBusyConfirmingOperation = false;

            //this.IsNewOperationAvailable =
            //    this.WmsDataProvider.IsEnabled
            //    &&
            //    this.MissionOperationsService.ActiveWmsMission != null
            //    &&
            //    this.MissionOperationsService.ActiveWmsOperation != null
            //    &&
            //    (
            //        this.MissionOperationsService.ActiveWmsOperation.Type != MissionOperationType.LoadingUnitCheck
            //        ||
            //        this.MissionOperationsService.ActiveWmsMission.Operations.Any(o =>
            //            o.Status != MissionOperationStatus.Completed
            //            &&
            //            o.Id != this.MissionOperationsService.ActiveWmsOperation.Id));
            if (!this.WmsDataProvider.IsEnabled
                ||
                !this.MachineModeService.IsWmsEnabled)
            {
                this.IsNewOperationAvailable = false;
            }
            else if (
                this.MissionOperationsService.ActiveWmsMission is null
                ||
                this.MissionOperationsService.ActiveWmsOperation is null)
            {
                this.IsNewOperationAvailable = false;
            }
            else
            {
                this.IsNewOperationAvailable =
                   this.MissionOperationsService.ActiveWmsMission.Operations.Any(o =>
                        o.Status == MissionOperationStatus.Executing
                        &&
                        o.Id != this.MissionOperationsService.ActiveWmsOperation.Id
                        &&
                        this.MissionOperationsService.ActiveWmsOperation.Id != 0);
            }

            //if (this.MissionOperationsService.ActiveMachineMission is null)
            //{
            //    this.NavigationService.GoBackTo(
            //        nameof(Utils.Modules.Operator),
            //        Utils.Modules.Operator.ItemOperations.WAIT);

            //}

            this.isUpperPosition = this.MachineService.Bay.Positions.Any(p => p.IsUpper && p.LoadingUnit?.Id == this.LoadingUnit?.Id);
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

            this.ItemSerialNumberVisibility = this.items.Any(s => s.ItemSerialNumber != null);
            this.RaisePropertyChanged(nameof(this.ItemSerialNumberVisibility));

            this.ItemLotVisibility = this.items.Any(s => s.Lot != null);
            this.RaisePropertyChanged(nameof(this.ItemLotVisibility));
        }

        private async void SetItemsAndCompartment()
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
                if (this.items.FirstOrDefault(ic => ic.ItemId == this.selectedItemCompartment.ItemId
                    && (this.selectedItemCompartment.Lot == null || this.selectedItemCompartment.Lot == ic.Lot)
                    && (this.selectedItemCompartment.ItemSerialNumber == null || this.selectedItemCompartment.ItemSerialNumber == ic.ItemSerialNumber)) is CompartmentDetails newSelectedItem)
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

            var activeOperation = this.MissionOperationsService.ActiveWmsOperation;
            if (activeOperation != null && activeOperation.ItemId > 0 && activeOperation.CompartmentId == this.selectedCompartment.Id)
            {
                this.selectedItem = this.Items.FirstOrDefault(ic => ic.ItemId == activeOperation.ItemId
                    && (activeOperation.Lot == null || ic.Lot == activeOperation.Lot)
                    && (activeOperation.SerialNumber == null || ic.ItemSerialNumber == activeOperation.SerialNumber));
            }

            if (this.itemsCompartments.FirstOrDefault(ic => ic.Id == this.selectedCompartment.Id
                                                        &&
                                                        ic.ItemId == this.selectedItem.ItemId
                                                        &&
                                                        ic.Stock == this.selectedItem.Stock
                                                        &&
                                                        (this.selectedItem.Lot == null || ic.Lot == this.selectedItem.Lot)
                                                        &&
                                                        ic.ItemSerialNumber == this.selectedItem.ItemSerialNumber) is CompartmentDetails newSelectedItemCompartment)
            //if (this.selectedItemCompartment != null && this.itemsCompartments.Any(ic => ic.Id == this.selectedItemCompartment.Id))
            {
                this.selectedItemCompartment = newSelectedItemCompartment;
                this.currentItemCompartmentIndex = this.itemsCompartments.ToList().IndexOf(this.selectedItemCompartment);
                this.RaisePropertyChanged();
            }

            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
