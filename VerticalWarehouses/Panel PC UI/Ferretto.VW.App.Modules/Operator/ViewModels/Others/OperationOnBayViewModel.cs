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

        private Bay bay;

        private bool canSave;

        private bool inventory;

        private bool isBusy;

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
            IMachineIdentityWebService identityService)
            : base(PresentationMode.Operator)
        {
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        }

        #endregion

        #region Properties

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

                this.IsEnableHandlingItemOperations = await this.identityService.IsEnableHandlingItemOperationsAsync();
                this.IsUpdatingStockByDifference = await this.identityService.IsUpdatingStockByDifferenceAsync();
                this.IsRequestConfirmForLastOperationOnLoadingUnit = await this.identityService.IsRequestConfirmForLastOperationOnLoadingUnitAsync();
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

                //await this.machineBaysWebService.SetAllOpertionBayAsync(this.pick, this.put, this.view, this.inventory, this.bay.Id);

                await this.identityService.SetBayOperationParamsAsync(this.IsEnableHandlingItemOperations, this.IsUpdatingStockByDifference, this.IsRequestConfirmForLastOperationOnLoadingUnit);

                this.Logger.Debug($"SetBayOperationParams: IsEnableHandlingItemOperations = {this.IsEnableHandlingItemOperations}; IsUpdatingStockByDifference = {this.IsUpdatingStockByDifference}; IsRequestConfirmForLastOperationOnLoadingUnit = {this.IsRequestConfirmForLastOperationOnLoadingUnit} ");
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
