using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class OperationOnBayViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineIdentityWebService identityService;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private bool barcodeAutomaticPut;

        private Bay bay;

        private bool canSave;

        private bool fixedPick;

        private bool inventory;

        private bool isAggregateList;

        private bool isBox;

        private bool isBusy;

        private bool isCarrefour;

        private bool isCheckListContinueInOtherMachine;

        private bool isDisableQtyItemEditingPick;

        private bool isDoubleConfirmBarcodeInventory;

        private bool isDoubleConfirmBarcodePick;

        private bool isDoubleConfirmBarcodePut;

        private bool isDrapery;

        private bool isEnableAddItem;

        private bool isEnableAddItemByList;

        private bool isEnableHandlingItemOperations;

        private bool isEnableNoteRules;

        private bool isListPickConfirm;

        private bool isListPutConfirm;

        private bool isLocalMachineItems;

        private bool isNrLabelsEditable;

        private bool isOrderList;

        private bool isOstec;

        private bool isQuantityLimited;

        private bool isRequestConfirmForLastOperationOnLoadingUnit;

        private bool isShowBarcodeImage;

        private bool isUpdatingStockByDifference;

        private bool isWaitingList;

        private bool isWaitingListPriorityHighlighted;

        private int itemUniqueIdLength;

        private bool operationRightToLeft;

        private bool pick;

        private bool put;

        private DelegateCommand saveSettingsCommand;

        private bool showQuantityOnInventory;

        private int toteBarcodeLength;

        private bool view;

        private int waitingListPriorityHighlighted;

        #endregion

        #region Constructors

        public OperationOnBayViewModel(IMachineBaysWebService machineBaysWebService,
            IMachineIdentityWebService identityService,
            IMachineConfigurationWebService machineConfigurationWebService)
            : base(PresentationMode.Operator)
        {
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
        }

        #endregion

        #region Properties

        public bool BarcodeAutomaticPut
        {
            get => this.barcodeAutomaticPut;
            set => this.SetProperty(ref this.barcodeAutomaticPut, value, this.CanExecute);
        }

        public Bay Bay
        {
            get => this.bay;
            set => this.SetProperty(ref this.bay, value, this.CanExecute);
        }

        public bool FixedPick
        {
            get => this.fixedPick;
            set => this.SetProperty(ref this.fixedPick, value, this.CanExecute);
        }

        public bool Inventory
        {
            get => this.inventory;
            set => this.SetProperty(ref this.inventory, value, this.CanExecute);
        }

        public bool IsAggregateList
        {
            get => this.isAggregateList;
            set => this.SetProperty(ref this.isAggregateList, value, this.CanExecute);
        }

        public bool IsBox
        {
            get => this.isBox;
            set => this.SetProperty(ref this.isBox, value, this.CanExecute);
        }

        public bool IsCarrefour
        {
            get => this.isCarrefour;
            set => this.SetProperty(ref this.isCarrefour, value, this.CanExecute);
        }

        public bool IsCheckListContinueInOtherMachine
        {
            get => this.isCheckListContinueInOtherMachine;
            set => this.SetProperty(ref this.isCheckListContinueInOtherMachine, value, this.CanExecute);
        }

        public bool IsDisableQtyItemEditingPick
        {
            get => this.isDisableQtyItemEditingPick;
            set => this.SetProperty(ref this.isDisableQtyItemEditingPick, value, this.CanExecute);
        }

        // not used
        public bool IsDoubleConfirmBarcodeInventory
        {
            get => this.isDoubleConfirmBarcodeInventory;
            set => this.SetProperty(ref this.isDoubleConfirmBarcodeInventory, value, this.CanExecute);
        }

        public bool IsDoubleConfirmBarcodePick
        {
            get => this.isDoubleConfirmBarcodePick;
            set => this.SetProperty(ref this.isDoubleConfirmBarcodePick, value, this.CanExecute);
        }

        public bool IsDoubleConfirmBarcodePut
        {
            get => this.isDoubleConfirmBarcodePut;
            set => this.SetProperty(ref this.isDoubleConfirmBarcodePut, value, this.CanExecute);
        }

        public bool IsDrapery
        {
            get => this.isDrapery;
            set => this.SetProperty(ref this.isDrapery, value, this.CanExecute);
        }

        public bool IsEnabeNoteRules
        {
            get => this.isEnableNoteRules;
            set => this.SetProperty(ref this.isEnableNoteRules, value, this.CanExecute);
        }

        public bool IsEnableAddItem
        {
            get => this.isEnableAddItem;
            set => this.SetProperty(ref this.isEnableAddItem, value, this.CanExecute);
        }

        public bool IsEnableAddItemByList
        {
            get => this.isEnableAddItemByList;
            set => this.SetProperty(ref this.isEnableAddItemByList, value, this.CanExecute);
        }

        public bool IsEnableHandlingItemOperations
        {
            get => this.isEnableHandlingItemOperations;
            set => this.SetProperty(ref this.isEnableHandlingItemOperations, value, this.CanExecute);
        }

        public bool IsListPickConfirm
        {
            get => this.isListPickConfirm;
            set => this.SetProperty(ref this.isListPickConfirm, value, this.CanExecute);
        }

        public bool IsListPutConfirm
        {
            get => this.isListPutConfirm;
            set => this.SetProperty(ref this.isListPutConfirm, value, this.CanExecute);
        }

        public bool IsLocalMachineItems
        {
            get => this.isLocalMachineItems;
            set => this.SetProperty(ref this.isLocalMachineItems, value, this.CanExecute);
        }

        public bool IsNrLabelsEditable
        {
            get => this.isNrLabelsEditable;
            set => this.SetProperty(ref this.isNrLabelsEditable, value, this.CanExecute);
        }

        public bool IsOrderList
        {
            get => this.isOrderList;
            set => this.SetProperty(ref this.isOrderList, value, this.CanExecute);
        }

        public bool IsOstec
        {
            get => this.isOstec;
            set => this.SetProperty(ref this.isOstec, value, this.CanExecute);
        }

        public bool IsQuantityLimited
        {
            get => this.isQuantityLimited;
            set => this.SetProperty(ref this.isQuantityLimited, value, this.CanExecute);
        }

        public bool IsRequestConfirmForLastOperationOnLoadingUnit
        {
            get => this.isRequestConfirmForLastOperationOnLoadingUnit;
            set => this.SetProperty(ref this.isRequestConfirmForLastOperationOnLoadingUnit, value, this.CanExecute);
        }

        public bool IsShowBarcodeImage
        {
            get => this.isShowBarcodeImage;
            set => this.SetProperty(ref this.isShowBarcodeImage, value, this.CanExecute);
        }

        public bool IsUpdatingStockByDifference
        {
            get => this.isUpdatingStockByDifference;
            set => this.SetProperty(ref this.isUpdatingStockByDifference, value, this.CanExecute);
        }

        public bool IsWaitingList
        {
            get => this.isWaitingList;
            set => this.SetProperty(ref this.isWaitingList, value, this.CanExecute);
        }

        public bool IsWaitingListPriorityHighlighted
        {
            get => this.isWaitingListPriorityHighlighted;
            set => this.SetProperty(ref this.isWaitingListPriorityHighlighted, value, this.CanExecute);
        }

        public int ItemUniqueIdLength
        {
            get => this.itemUniqueIdLength;
            set => this.SetProperty(ref this.itemUniqueIdLength, value, this.CanExecute);
        }

        public bool OperationRightToLeft
        {
            get => this.operationRightToLeft;
            set => this.SetProperty(ref this.operationRightToLeft, value, this.CanExecute);
        }

        public bool Pick
        {
            get => this.pick;
            set => this.SetProperty(ref this.pick, value, this.CanExecute);
        }

        public bool Put
        {
            get => this.put;
            set => this.SetProperty(ref this.put, value, this.CanExecute);
        }

        public ICommand SaveSettingsCommand => this.saveSettingsCommand
            ??
            (this.saveSettingsCommand = new DelegateCommand(
                async () => await this.SaveSettingsAsync(),
                this.CanSave));

        public bool ShowQuantityOnInventory
        {
            get => this.showQuantityOnInventory;
            set => this.SetProperty(ref this.showQuantityOnInventory, value, this.CanExecute);
        }

        public int ToteBarcodeLength
        {
            get => this.toteBarcodeLength;
            set => this.SetProperty(ref this.toteBarcodeLength, value, this.CanExecute);
        }

        public bool View
        {
            get => this.view;
            set => this.SetProperty(ref this.view, value, this.CanExecute);
        }

        public int WaitingListPriorityHighlighted
        {
            get => this.waitingListPriorityHighlighted;
            set => this.SetProperty(ref this.waitingListPriorityHighlighted, value, this.CanExecute);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await this.LoadData();

            this.canSave = false;

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.saveSettingsCommand?.RaiseCanExecuteChanged();
        }

        private void CanExecute()
        {
            this.canSave = true;
            this.RaiseCanExecuteChanged();
        }

        private bool CanSave()
        {
            return !this.MachineStatus.IsMoving &&
                !this.isBusy &&
                this.canSave;
        }

        private async Task LoadData()
        {
            try
            {
                this.isBusy = true;
                this.Bay = this.MachineService.Bay;
                this.Pick = this.Bay.Pick;
                this.Put = this.Bay.Put;
                this.View = this.Bay.View;
                this.Inventory = this.Bay.Inventory;
                this.BarcodeAutomaticPut = this.Bay.BarcodeAutomaticPut;
                this.IsShowBarcodeImage = this.Bay.ShowBarcodeImage is true;
                this.IsCheckListContinueInOtherMachine = this.Bay.CheckListContinueInOtherMachine is true;

                var configuration = await this.machineConfigurationWebService.GetConfigAsync();
                this.IsEnableHandlingItemOperations = configuration.IsEnableHandlingItemOperations;
                this.IsUpdatingStockByDifference = configuration.IsUpdatingStockByDifference;
                this.IsRequestConfirmForLastOperationOnLoadingUnit = configuration.IsRequestConfirmForLastOperationOnLoadingUnit;
                this.IsEnableAddItem = configuration.IsEnableAddItem;
                this.IsEnableAddItemByList = configuration.IsAddItemByList;
                this.IsCarrefour = configuration.IsCarrefour;
                this.IsOstec = configuration.IsOstec;
                this.IsWaitingList = configuration.IsWaitingListFiltered is true;
                this.IsDrapery = configuration.IsDrapery;
                this.IsDisableQtyItemEditingPick = configuration.IsDisableQtyItemEditingPick;
                this.IsDoubleConfirmBarcodeInventory = configuration.IsDoubleConfirmBarcodeInventory;
                this.IsDoubleConfirmBarcodePick = configuration.IsDoubleConfirmBarcodePick;
                this.IsDoubleConfirmBarcodePut = configuration.IsDoubleConfirmBarcodePut;
                this.IsBox = configuration.Box;
                this.IsEnabeNoteRules = configuration.EnabeNoteRules;
                this.IsLocalMachineItems = configuration.IsLocalMachineItems;
                this.IsOrderList = configuration.IsOrderList;
                this.IsQuantityLimited = configuration.IsQuantityLimited;
                this.ItemUniqueIdLength = configuration.ItemUniqueIdLength;
                this.ToteBarcodeLength = configuration.ToteBarcodeLength;
                this.ShowQuantityOnInventory = configuration.ShowQuantityOnInventory;
                this.OperationRightToLeft = configuration.OperationRightToLeft;
                this.FixedPick = configuration.FixedPick;

                this.WaitingListPriorityHighlighted = configuration.WaitingListPriorityHighlighted.Value;
                this.IsListPickConfirm = configuration.ListPickConfirm;
                this.IsListPutConfirm = configuration.ListPutConfirm;
                this.IsWaitingListPriorityHighlighted = this.WaitingListPriorityHighlighted != -1;

                this.IsAggregateList = configuration.AggregateList;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.isBusy = false;
            }
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                this.isBusy = true;
                this.IsWaitingForResponse = true;

                await this.machineBaysWebService.SetAllOperationsBayAsync(this.Pick, this.Put, this.View, this.Inventory, this.BarcodeAutomaticPut, this.bay.Id, this.IsShowBarcodeImage, this.IsCheckListContinueInOtherMachine, this.IsNrLabelsEditable);

                var machine = new Machine();
                machine.IsEnableHandlingItemOperations = this.IsEnableHandlingItemOperations;
                machine.IsUpdatingStockByDifference = this.IsUpdatingStockByDifference;
                machine.IsRequestConfirmForLastOperationOnLoadingUnit = this.IsRequestConfirmForLastOperationOnLoadingUnit;
                machine.IsEnableAddItem = this.IsEnableAddItem;
                machine.IsAddItemByList = this.IsEnableAddItemByList;
                machine.IsCarrefour = this.IsCarrefour;
                machine.IsOstec = this.IsOstec;
                machine.IsWaitingListFiltered = this.IsWaitingList;
                machine.IsDisableQtyItemEditingPick = this.IsDisableQtyItemEditingPick;
                machine.IsDoubleConfirmBarcodeInventory = this.IsDoubleConfirmBarcodeInventory;
                machine.IsDoubleConfirmBarcodePick = this.IsDoubleConfirmBarcodePick;
                machine.IsDoubleConfirmBarcodePut = this.IsDoubleConfirmBarcodePut;
                machine.Box = this.IsBox;
                machine.EnabeNoteRules = this.IsEnabeNoteRules;
                machine.IsLocalMachineItems = this.IsLocalMachineItems;
                machine.IsOrderList = this.IsOrderList;
                machine.ItemUniqueIdLength = this.ItemUniqueIdLength;
                machine.ToteBarcodeLength = this.ToteBarcodeLength;
                machine.IsDrapery = this.IsDrapery;
                machine.IsQuantityLimited = this.IsQuantityLimited;
                machine.ShowQuantityOnInventory = this.ShowQuantityOnInventory;
                machine.OperationRightToLeft = this.OperationRightToLeft;
                machine.FixedPick = this.FixedPick;

                machine.WaitingListPriorityHighlighted = this.IsWaitingListPriorityHighlighted && this.WaitingListPriorityHighlighted >= 0 ? this.WaitingListPriorityHighlighted : -1;
                machine.ListPutConfirm = this.IsListPutConfirm;
                machine.ListPickConfirm = this.IsListPickConfirm;

                machine.AggregateList = this.IsAggregateList;

                await this.identityService.SetBayOperationParamsAsync(machine);

                this.Logger.Debug($"SetBayOperationParams: IsEnableHandlingItemOperations = {this.IsEnableHandlingItemOperations}; " +
                    $"IsUpdatingStockByDifference = {this.IsUpdatingStockByDifference}; " +
                    $"IsRequestConfirmForLastOperationOnLoadingUnit = {this.IsRequestConfirmForLastOperationOnLoadingUnit};" +
                    $"IsEnableAddItem = {this.IsEnableAddItem};" +
                    $"IsEnableAddItem = {this.IsEnableAddItemByList};" +
                    $"IsDisableQtyItemEditingPick = {this.IsDisableQtyItemEditingPick} ");
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                await this.LoadData();

                this.canSave = false;

                this.IsWaitingForResponse = false;

                this.isBusy = false;
            }
        }

        #endregion
    }
}
