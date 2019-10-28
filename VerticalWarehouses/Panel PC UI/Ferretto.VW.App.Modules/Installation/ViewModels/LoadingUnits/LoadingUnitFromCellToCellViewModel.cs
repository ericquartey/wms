using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class LoadingUnitFromCellToCellViewModel : BaseCellMovementsViewModel
    {
        #region Constructors

        public LoadingUnitFromCellToCellViewModel(
                    IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
                    IMachineCellsWebService machineCellsWebService,
                    IBayManager bayManagerService)
            : base(
                machineLoadingUnitsWebService,
                machineCellsWebService,
                bayManagerService)
        {
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
            await this.RetrieveLoadingUnitsAsync();
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

                if (this.LoadingUnitCellId == null)
                {
                    this.ShowNotification("Id cella su cassetto inserito non valido", Services.Models.NotificationSeverity.Warning);
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

                this.IsWaitingForResponse = true;

                await this.MachineLoadingUnitsWebService.StartMovingSourceDestinationAsync(LoadingUnitLocation.Cell, LoadingUnitLocation.Cell, this.LoadingUnitCellId, this.DestinationCellId);
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
