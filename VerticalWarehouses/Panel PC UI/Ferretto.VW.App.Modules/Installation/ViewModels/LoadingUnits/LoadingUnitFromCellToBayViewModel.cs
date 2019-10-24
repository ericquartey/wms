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
                    IMachineElevatorWebService machineElevatorWebService,
                    IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
                    IMachineSensorsWebService machineSensorsWebService,
                    IBayManager bayManagerService)
            : base(
                machineElevatorWebService,
                machineLoadingUnitsWebService,
                machineSensorsWebService,
                bayManagerService)
        {
        }

        #endregion

        #region Properties

        public ICommand ConfirmEjectLoadingUnitCommand =>
                this.confirmEjectLoadingUnitCommand
                ??
                (this.confirmEjectLoadingUnitCommand = new DelegateCommand(this.ConfirmEjectLoadingUnit, this.CanConfirmEjectLoadingUnit));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
            await this.RetrieveLoadingUnitsAsync();
            this.LoadingUnitId = null;
            this.SelectBayPosition1();
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

                //await this.machineLoadingUnitsWebService.StartMovingSourceDestinationAsync(source, LoadingUnitDestination.Cell, null, null);
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

        private bool CanConfirmEjectLoadingUnit()
        {
            return this.isEjectLoadingUnitConfirmationEnabled;
        }

        private void ConfirmEjectLoadingUnit()
        {
            // TODO call resume
        }

        #endregion
    }
}
