using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class FromBayToCellViewModel : BaseMovementsViewModel
    {
        #region Fields

        private int? loadingUnitId;

        private IEnumerable<LoadingUnit> loadingUnits;

        #endregion

        #region Constructors

        public FromBayToCellViewModel(
                    IMachineDepositAndPickupProcedureWebService machineDepositPickupProcedure,
                    IMachineElevatorWebService machineElevatorWebService,
                    IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
                    IMachineSensorsWebService machineSensorsWebService,
                    IBayManager bayManagerService)
            : base(
                machineDepositPickupProcedure,
                machineElevatorWebService,
                machineLoadingUnitsWebService,
                machineSensorsWebService,
                bayManagerService)
        {
        }

        #endregion

        #region Properties

        public bool IsLoadingUnitIdValid
        {
            get
            {
                if (!this.loadingUnitId.HasValue)
                {
                    return false;
                }

                return this.loadingUnits.Any(l => l.Id == this.loadingUnitId.Value);
            }
        }

        public int? LoadingUnitId
        {
            get => this.loadingUnitId;
            set
            {
                if (this.SetProperty(ref this.loadingUnitId, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Methods

        public virtual bool CanExecuteStartCommand()
        {
            return
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsLoadingUnitInBay
                &&
                !this.IsLoadingUnitOnElevator
                &&
                this.ShutterSensors.Closed;
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
            await this.RetrieveLoadingUnitsAsync();
        }

        public async Task RetrieveLoadingUnitsAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.loadingUnits = await this.MachineLoadingUnitsWebService.GetAllAsync();
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

        public override async Task StartAsync()
        {
            try
            {
                if (!this.IsLoadingUnitIdValid)
                {
                    this.ShowNotification("Id cassetto inserito non valido", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                var source = this.GetLoadingUnitSource();

                if (source == MAS.AutomationService.Contracts.LoadingUnitDestination.NoDestination)
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

        #endregion
    }
}
