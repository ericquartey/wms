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
        #region Fields

        private bool isPosition1Selected;

        private bool isPosition2Selected;

        private DelegateCommand selectBayPosition1Command;

        private DelegateCommand selectBayPosition2Command;

        #endregion

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

                var source = this.GetLoadingUnitSource();

                if (source == LoadingUnitLocation.NoLocation)
                {
                    this.ShowNotification("Tipo scelta sorgente non valida", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsWaitingForResponse = true;

                // await this.MachineLoadingUnitsWebService.StartMovingSourceDestinationAsync(source, LoadingUnitDestination.Cell, this.LoadingUnitId, this.DestinationCellId);
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

        private MAS.AutomationService.Contracts.LoadingUnitLocation GetLoadingUnitSource()
        {
            if (this.Bay.Number == BayNumber.BayOne)
            {
                if (this.IsPosition1Selected)
                {
                    return LoadingUnitLocation.InternalBay1Up;
                }
                else
                {
                    return LoadingUnitLocation.InternalBay1Down;
                }
            }

            if (this.Bay.Number == BayNumber.BayTwo)
            {
                if (this.IsPosition1Selected)
                {
                    return LoadingUnitLocation.InternalBay2Up;
                }
                else
                {
                    return LoadingUnitLocation.InternalBay2Down;
                }
            }

            if (this.Bay.Number == BayNumber.BayThree)
            {
                if (this.IsPosition1Selected)
                {
                    return LoadingUnitLocation.InternalBay3Up;
                }
                else
                {
                    return LoadingUnitLocation.InternalBay3Down;
                }
            }

            return LoadingUnitLocation.NoLocation;
        }

        private void SelectBayPosition1()
        {
            this.IsPosition1Selected = true;
            if (this.Bay.Positions.FirstOrDefault() is BayPosition bayPosition)
            {
                this.LoadingUnitId = bayPosition.LoadingUnit?.Id;
            }
        }

        private void SelectBayPosition2()
        {
            this.IsPosition2Selected = true;
            if (this.Bay.Positions.LastOrDefault() is BayPosition bayPosition)
            {
                this.LoadingUnitId = bayPosition.LoadingUnit?.Id;
            }
        }

        #endregion
    }
}
