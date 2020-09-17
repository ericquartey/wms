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

        private readonly IMachineBaysWebService machineBaysWebService;

        private Bay bay;

        private bool inventory;

        private bool pick;

        private bool put;

        private DelegateCommand saveSettingsCommand;

        private bool view;

        #endregion

        #region Constructors

        public OperationOnBayViewModel(IMachineBaysWebService machineBaysWebService)
            : base(PresentationMode.Operator)
        {
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
        }

        #endregion

        #region Properties

        public Bay Bay
        {
            get => this.bay;
            set => this.SetProperty(ref this.bay, value, this.RaiseCanExecuteChanged);
        }

        public bool Inventory
        {
            get => this.inventory;
            set => this.SetProperty(ref this.inventory, value, this.RaiseCanExecuteChanged);
        }

        public bool Pick
        {
            get => this.pick;
            set => this.SetProperty(ref this.pick, value, this.RaiseCanExecuteChanged);
        }

        public bool Put
        {
            get => this.put;
            set => this.SetProperty(ref this.put, value, this.RaiseCanExecuteChanged);
        }

        public ICommand SaveSettingsCommand =>
                                            this.saveSettingsCommand
            ??
            (this.saveSettingsCommand = new DelegateCommand(
                async () => await this.SaveSettingsAsync(),
                this.CanSave));

        public bool View
        {
            get => this.view;
            set => this.SetProperty(ref this.view, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.LoadData();

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.saveSettingsCommand?.RaiseCanExecuteChanged();
        }

        private bool CanSave()
        {
            return !this.MachineStatus.IsMoving;
        }

        private void LoadData()
        {
            try
            {
                this.Bay = this.MachineService.Bay;
                var bays = this.MachineService.Bays;
                this.Pick = this.Bay.Pick;
                this.Put = this.Bay.Put;
                this.View = this.Bay.View;
                this.Inventory = this.Bay.Inventory;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineBaysWebService.SetAllOpertionBayAsync(this.pick, this.put, this.view, this.inventory, this.bay.Id);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.LoadData();

                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
