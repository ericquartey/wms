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
        private bool isPosition1Selected;

        private bool isPosition2Selected;

        private bool isPosition3Selected;

        private int? cellId;

        private IEnumerable<Cell> cells;

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

        public bool IsPosition1Selected
        {
            get => this.isPosition1Selected;
            set
            {
                if (this.SetProperty(ref this.isPosition1Selected, value) && value)
                {
                    this.IsPosition2Selected = false;
                    this.IsPosition3Selected = false;
                }
            }
        }

        public bool IsPosition2Selected
        {
            get => this.isPosition2Selected;
            set
            {
                if (this.SetProperty(ref this.isPosition2Selected, value) && value)
                {
                    this.IsPosition1Selected = false;
                    this.IsPosition3Selected = false;
                }
            }
        }

        public bool IsPosition3Selected
        {
            get => this.isPosition3Selected;
            set
            {
                if (this.SetProperty(ref this.isPosition3Selected, value)
                    &&
                    value)
                {
                    this.IsPosition1Selected = false;
                    this.IsPosition2Selected = false;
                }
            }
        }

        public int? CellId
        {
            get => this.cellId;
            set
            {
                if (this.SetProperty(ref this.cellId, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        protected IEnumerable<Cell> Cells
        {
            get => this.cells;
            private set
            {
                if (this.SetProperty(ref this.cells, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsCellFree
        {
            get
            {
                if (!this.cellId.HasValue)
                {
                    return false;
                }

                var cellFound = this.cells.FirstOrDefault(l => l.Id == this.cellId.Value);
                if (!(cellFound is null))
                {
                    return cellFound.Status == CellStatus.Free;
                }

                return false;
            }
        }

        public bool IsCellIdValid
        {
            get
            {
                if (!this.cellId.HasValue)
                {
                    return false;
                }

                return this.cells.Any(l => l.Id == this.cellId.Value);
            }
        }

        #endregion

        #region Methods

        private MAS.AutomationService.Contracts.LoadingUnitDestination GetLoadingUnitSource()
        {
            if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayOne)
            {
                if (this.IsPosition1Selected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitDestination.InternalBay1Up;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitDestination.InternalBay1Down;
                }
            }

            if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayTwo)
            {
                if (this.IsPosition2Selected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitDestination.InternalBay2Up;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitDestination.InternalBay2Down;
                }
            }

            if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayThree)
            {
                if (this.IsPosition3Selected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitDestination.InternalBay3Up;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitDestination.InternalBay3Down;
                }
            }

            return MAS.AutomationService.Contracts.LoadingUnitDestination.NoDestination;
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

                var source = this.GetLoadingUnitSource();

                if (source == MAS.AutomationService.Contracts.LoadingUnitDestination.NoDestination)
                {
                    this.ShowNotification("Tipo scelta sorgente non valida", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsWaitingForResponse = true;

                await this.MachineLoadingUnitsWebService.StartMovingSourceDestinationAsync(source, LoadingUnitDestination.Cell, null, this.CellId.Value);
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
