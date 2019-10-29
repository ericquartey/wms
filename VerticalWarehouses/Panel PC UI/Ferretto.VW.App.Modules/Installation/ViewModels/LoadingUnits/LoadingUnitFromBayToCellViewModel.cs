using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    internal sealed class LoadingUnitFromBayToCellViewModel : BaseCellMovementsViewModel
    {
        #region Constructors

        public LoadingUnitFromBayToCellViewModel(
                    IMachineElevatorWebService machineElevatorWebService,
                    IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
                    IMachineSensorsWebService machineSensorsWebService,
                    IMachineCellsWebService machineCellsWebService,
                    IBayManager bayManagerService)
            : base(
                machineElevatorWebService,
                machineLoadingUnitsWebService,
                machineSensorsWebService,
                machineCellsWebService,
                bayManagerService)

        {
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

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

                if (!this.IsCellIdValid)
                {
                    this.ShowNotification("Id cella inserito non valido", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                if (!this.IsCellFree)
                {
                    this.ShowNotification("la cella inserita non è libera", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                if (!this.IsLoadingUnitInBay)
                {
                    this.ShowNotification("la baia non è occupata", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                var source = this.GetLoadingUnitSource();

                if (source == LoadingUnitLocation.NoLocation)
                {
                    this.ShowNotification("Tipo scelta sorgente non valida", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsWaitingForResponse = true;

                //await this.MachineLoadingUnitsWebService.StartMovingSourceDestinationAsync(source, LoadingUnitDestination.Cell, this.LoadingUnitId, this.DestinationCellId);
                await this.MachineLoadingUnitsWebService.InsertLoadingUnitAsync(source, this.DestinationCellId.Value, this.LoadingUnitId.Value);
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
