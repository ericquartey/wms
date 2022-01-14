using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
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

        private bool isBusy;

        private bool isDisableQtyItemEditingPick;

        private bool isEnableAddItem;

        private bool isEnableHandlingItemOperations;

        private bool isRequestConfirmForLastOperationOnLoadingUnit;

        private bool isUpdatingStockByDifference;

        private bool pick;

        private bool put;

        private DelegateCommand saveSettingsCommand;

        private bool view;

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

        public bool IsDisableQtyItemEditingPick
        {
            get => this.isDisableQtyItemEditingPick;
            set => this.SetProperty(ref this.isDisableQtyItemEditingPick, value, this.CanExecute);
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

        public bool IsRequestConfirmForLastOperationOnLoadingUnit
        {
            get => this.isRequestConfirmForLastOperationOnLoadingUnit;
            set => this.SetProperty(ref this.isRequestConfirmForLastOperationOnLoadingUnit, value, this.CanExecute);
        }

        public bool IsUpdatingStockByDifference
        {
            get => this.isUpdatingStockByDifference;
            set => this.SetProperty(ref this.isUpdatingStockByDifference, value, this.CanExecute);
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

                var configuration = await this.machineConfigurationWebService.GetAsync();
                this.IsEnableHandlingItemOperations = configuration.Machine.IsEnableHandlingItemOperations;
                this.IsUpdatingStockByDifference = configuration.Machine.IsUpdatingStockByDifference;
                this.IsRequestConfirmForLastOperationOnLoadingUnit = configuration.Machine.IsRequestConfirmForLastOperationOnLoadingUnit;
                this.IsEnableAddItem = configuration.Machine.IsEnableAddItem;
                this.IsDisableQtyItemEditingPick = configuration.Machine.IsDisableQtyItemEditingPick;
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

                await this.machineBaysWebService.SetAllOperationsBayAsync(this.Pick, this.Put, this.View, this.Inventory, this.BarcodeAutomaticPut, this.bay.Id);

                await this.identityService.SetBayOperationParamsAsync(
                    this.IsEnableHandlingItemOperations,
                    this.IsUpdatingStockByDifference,
                    this.IsRequestConfirmForLastOperationOnLoadingUnit,
                    this.IsEnableAddItem,
                    this.IsDisableQtyItemEditingPick);

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
