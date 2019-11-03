using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class LoadingUnitFromCellToBayViewModel : BaseMovementsViewModel
    {
        #region Fields

        private DelegateCommand confirmEjectLoadingUnitCommand;

        private bool isEjectLoadingUnitConfirmationEnabled;

        #endregion

        #region Constructors

        public LoadingUnitFromCellToBayViewModel(
                    IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
                    IBayManager bayManagerService)
            : base(
                machineLoadingUnitsWebService,
                bayManagerService)
        {
        }

        #endregion

        #region Properties

        public ICommand ConfirmEjectLoadingUnitCommand =>
                this.confirmEjectLoadingUnitCommand
                ??
                (this.confirmEjectLoadingUnitCommand = new DelegateCommand(async () => await this.ConfirmEjectLoadingUnit(), this.CanConfirmEjectLoadingUnit));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
            await this.RetrieveLoadingUnitsAsync();
            this.LoadingUnitId = null;
            this.SelectBayPositionDown();
        }

        public override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.confirmEjectLoadingUnitCommand?.RaiseCanExecuteChanged();
        }

        public override async Task StartAsync()
        {
            try
            {
                if (!this.IsLoadingUnitIdValid)
                {
                    this.ShowNotification("Id cassetto inserito non valido", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                var destination = this.GetLoadingUnitSource();

                if (destination == LoadingUnitLocation.NoLocation)
                {
                    this.ShowNotification("Tipo scelta sorgente non valida", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsWaitingForResponse = true;

                await this.MachineLoadingUnitsWebService.EjectLoadingUnitAsync(destination, this.LoadingUnitId.Value);
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

        protected override void Ended()
        {
            base.Ended();

            this.isEjectLoadingUnitConfirmationEnabled = false;

            this.confirmEjectLoadingUnitCommand.RaiseCanExecuteChanged();
        }

        protected override void OnWaitResume()
        {
            this.RaiseCanExecuteChanged();

            this.isEjectLoadingUnitConfirmationEnabled = true;

            this.confirmEjectLoadingUnitCommand.RaiseCanExecuteChanged();
        }

        private bool CanConfirmEjectLoadingUnit()
        {
            return this.isEjectLoadingUnitConfirmationEnabled;
        }

        private async Task ConfirmEjectLoadingUnit()
        {
            await this.MachineLoadingUnitsWebService.ResumeAsync(this.CurrentMissionId, this.Bay.Number);
        }

        #endregion
    }
}
