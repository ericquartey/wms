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

        private bool inventory;

        private bool isBox;

        private bool isBusy;

        private bool isCarrefour;

        private bool isDisableQtyItemEditingPick;

        private bool isDoubleConfirmBarcodeInventory;

        private bool isDoubleConfirmBarcodePick;

        private bool isDoubleConfirmBarcodePut;

        private bool isDrapery;

        private bool isEnableAddItem;

        private bool isEnableHandlingItemOperations;

        private bool isEnableNoteRules;

        private bool isLocalMachineItems;

        private bool isOrderList;

        private bool isRequestConfirmForLastOperationOnLoadingUnit;

        private bool isShowBarcodeImage;

        private bool isUpdatingStockByDifference;

        private int itemUniqueIdLength;

        private bool pick;

        private bool put;

        private DelegateCommand saveSettingsCommand;

        private int toteBarcodeLength;

        private bool view;

        private bool isCheckListContinueInOtherMachine;

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

        public bool Inventory
        {
            get => this.inventory;
            set => this.SetProperty(ref this.inventory, value, this.CanExecute);
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

        public bool IsEnableHandlingItemOperations
        {
            get => this.isEnableHandlingItemOperations;
            set => this.SetProperty(ref this.isEnableHandlingItemOperations, value, this.CanExecute);
        }

        public bool IsLocalMachineItems
        {
            get => this.isLocalMachineItems;
            set => this.SetProperty(ref this.isLocalMachineItems, value, this.CanExecute);
        }

        public bool IsOrderList
        {
            get => this.isOrderList;
            set => this.SetProperty(ref this.isOrderList, value, this.CanExecute);
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

        public bool IsCheckListContinueInOtherMachine
        {
            get => this.isCheckListContinueInOtherMachine;
            set => this.SetProperty(ref this.isCheckListContinueInOtherMachine, value, this.CanExecute);
        }

        public bool IsUpdatingStockByDifference
        {
            get => this.isUpdatingStockByDifference;
            set => this.SetProperty(ref this.isUpdatingStockByDifference, value, this.CanExecute);
        }

        public int ItemUniqueIdLength
        {
            get => this.itemUniqueIdLength;
            set => this.SetProperty(ref this.itemUniqueIdLength, value, this.CanExecute);
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
                this.Bay = await this.machineBaysWebService.GetByNumberAsync(this.MachineService.BayNumber);
                this.Pick = this.Bay.Pick;
                this.Put = this.Bay.Put;
                this.View = this.Bay.View;
                this.Inventory = this.Bay.Inventory;
                this.BarcodeAutomaticPut = this.Bay.BarcodeAutomaticPut;
                this.IsShowBarcodeImage = this.Bay.ShowBarcodeImage;
                this.IsCheckListContinueInOtherMachine = this.Bay.CheckListContinueInOtherMachine;

                var configuration = await this.machineConfigurationWebService.GetAsync();
                this.IsEnableHandlingItemOperations = configuration.Machine.IsEnableHandlingItemOperations;
                this.IsUpdatingStockByDifference = configuration.Machine.IsUpdatingStockByDifference;
                this.IsRequestConfirmForLastOperationOnLoadingUnit = configuration.Machine.IsRequestConfirmForLastOperationOnLoadingUnit;
                this.IsEnableAddItem = configuration.Machine.IsEnableAddItem;
                this.IsCarrefour = configuration.Machine.IsCarrefour;
                this.IsDrapery = configuration.Machine.IsDrapery;
                this.IsDisableQtyItemEditingPick = configuration.Machine.IsDisableQtyItemEditingPick;
                this.IsDoubleConfirmBarcodeInventory = configuration.Machine.IsDoubleConfirmBarcodeInventory;
                this.IsDoubleConfirmBarcodePick = configuration.Machine.IsDoubleConfirmBarcodePick;
                this.IsDoubleConfirmBarcodePut = configuration.Machine.IsDoubleConfirmBarcodePut;
                this.IsBox = configuration.Machine.Box;
                this.IsEnabeNoteRules = configuration.Machine.EnabeNoteRules;
                this.IsLocalMachineItems = configuration.Machine.IsLocalMachineItems;
                this.IsOrderList = configuration.Machine.IsOrderList;
                this.ItemUniqueIdLength = configuration.Machine.ItemUniqueIdLength;
                this.ToteBarcodeLength = configuration.Machine.ToteBarcodeLength;
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

                await this.machineBaysWebService.SetAllOperationsBayAsync(this.Pick, this.Put, this.View, this.Inventory, this.BarcodeAutomaticPut, this.bay.Id, this.IsShowBarcodeImage, this.IsCheckListContinueInOtherMachine);

                var machine = new Machine();
                machine.IsEnableHandlingItemOperations = this.IsEnableHandlingItemOperations;
                machine.IsUpdatingStockByDifference = this.IsUpdatingStockByDifference;
                machine.IsRequestConfirmForLastOperationOnLoadingUnit = this.IsRequestConfirmForLastOperationOnLoadingUnit;
                machine.IsEnableAddItem = this.IsEnableAddItem;
                machine.IsCarrefour = this.IsCarrefour;
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

                await this.identityService.SetBayOperationParamsAsync(machine);

                this.Logger.Debug($"SetBayOperationParams: IsEnableHandlingItemOperations = {this.IsEnableHandlingItemOperations}; " +
                    $"IsUpdatingStockByDifference = {this.IsUpdatingStockByDifference}; " +
                    $"IsRequestConfirmForLastOperationOnLoadingUnit = {this.IsRequestConfirmForLastOperationOnLoadingUnit};" +
                    $"IsEnableAddItem = {this.IsEnableAddItem};" +
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
