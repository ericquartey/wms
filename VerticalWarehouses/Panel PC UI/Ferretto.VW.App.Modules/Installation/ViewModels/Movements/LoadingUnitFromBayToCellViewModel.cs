using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class LoadingUnitFromBayToCellViewModel : BaseCellMovementsViewModel
    {
        #region Fields

        private bool isPosition1Selected;

        private bool isPosition2Selected;

        private DelegateCommand selectBayPosition1Command;

        private DelegateCommand selectBayPosition2Command;

        #endregion

        #region Constructors

        public LoadingUnitFromBayToCellViewModel(
                    IMachineDepositAndPickupProcedureWebService machineDepositPickupProcedure,
                    IMachineElevatorWebService machineElevatorWebService,
                    IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
                    IMachineSensorsWebService machineSensorsWebService,
                    IMachineCellsWebService machineCellsWebService,
                    IBayManager bayManagerService)
            : base(
                machineDepositPickupProcedure,
                machineElevatorWebService,
                machineLoadingUnitsWebService,
                machineSensorsWebService,
                machineCellsWebService,
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
                if (this.SetProperty(ref this.isPosition1Selected, value))
                {
                    this.IsPosition2Selected = !this.isPosition1Selected;
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
                    this.IsPosition2Selected = !this.isPosition1Selected;
                }
            }
        }

        public ICommand SelectBayPosition1Command =>
                        this.selectBayPosition1Command
                        ??
                        (this.selectBayPosition1Command = new DelegateCommand(this.SelectBayPosition1));

        public ICommand SelectBayPosition2Command =>
                        this.selectBayPosition2Command
                        ??
                        (this.selectBayPosition2Command = new DelegateCommand(this.SelectBayPosition2));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.LoadingUnitId = this.Bay?.LoadingUnit?.Id;
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

                await this.MachineLoadingUnitsWebService.StartMovingSourceDestinationAsync(source, LoadingUnitDestination.Cell, this.LoadingUnitId, this.DestinationCellId);
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
                if (this.IsPosition1Selected)
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

        private void SelectBayPosition1()
        {
            this.IsPosition1Selected = true;
        }

        private void SelectBayPosition2()
        {
            this.IsPosition2Selected = true;
        }

        #endregion
    }
}
