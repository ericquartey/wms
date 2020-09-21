using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    internal sealed class StatisticsViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private readonly IMachineCellsWebService machineCellsWebService;

        private readonly IMachineCompactingWebService machineCompactingWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private double fragmentBackPercent;

        private double fragmentFrontPercent;

        private double fragmentTotalPercent;

        private int totalDrawers;

        private int unitsInCell;

        private int unitsInBay;

        private int unitsInElevator;

        private int totalCells;

        private int busyCells;

        private int lockedCells;

        private int freeCells;

        private int freeCellsForSupport;

        private int freeCellsOnlySpace;

        #endregion

        #region Constructors

        public StatisticsViewModel(IMachineCompactingWebService machineCompactingWebService,
            IMachineCellsWebService machineCellsWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService)
            : base()
        {
            this.machineCompactingWebService = machineCompactingWebService ?? throw new ArgumentNullException(nameof(machineCompactingWebService));
            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
        }

        #endregion

        #region Properties

        public double FragmentBackPercent
        {
            get => this.fragmentBackPercent;
            set => this.SetProperty(ref this.fragmentBackPercent, value, this.RaiseCanExecuteChanged);
        }

        public double FragmentFrontPercent
        {
            get => this.fragmentFrontPercent;
            set => this.SetProperty(ref this.fragmentFrontPercent, value, this.RaiseCanExecuteChanged);
        }

        public double FragmentTotalPercent
        {
            get => this.fragmentTotalPercent;
            set => this.SetProperty(ref this.fragmentTotalPercent, value, this.RaiseCanExecuteChanged);
        }

        public int TotalDrawers
        {
            get => this.totalDrawers;
            private set => this.SetProperty(ref this.totalDrawers, value, this.RaiseCanExecuteChanged);
        }

        public int UnitsInCell
        {
            get => this.unitsInCell;
            private set => this.SetProperty(ref this.unitsInCell, value, this.RaiseCanExecuteChanged);
        }

        public int UnitsInBay
        {
            get => this.unitsInBay;
            private set => this.SetProperty(ref this.unitsInBay, value, this.RaiseCanExecuteChanged);
        }

        public int UnitsInElevator
        {
            get => this.unitsInElevator;
            private set => this.SetProperty(ref this.unitsInElevator, value, this.RaiseCanExecuteChanged);
        }

        public int TotalCells
        {
            get => this.totalCells;
            private set => this.SetProperty(ref this.totalCells, value, this.RaiseCanExecuteChanged);
        }

        public int BusyCells
        {
            get => this.busyCells;
            private set => this.SetProperty(ref this.busyCells, value, this.RaiseCanExecuteChanged);
        }

        public int LockedCells
        {
            get => this.lockedCells;
            private set => this.SetProperty(ref this.lockedCells, value, this.RaiseCanExecuteChanged);
        }

        public int FreeCells
        {
            get => this.freeCells;
            private set => this.SetProperty(ref this.freeCells, value, this.RaiseCanExecuteChanged);
        }

        public int FreeCellsForSupport
        {
            get => this.freeCellsForSupport;
            private set => this.SetProperty(ref this.freeCellsForSupport, value, this.RaiseCanExecuteChanged);
        }

        public int FreeCellsOnlySpace
        {
            get => this.freeCellsOnlySpace;
            private set => this.SetProperty(ref this.freeCellsOnlySpace, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            // TODO: Insert code here

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                var cells = await this.machineCellsWebService.GetStatisticsAsync();
                this.FragmentBackPercent = cells.FragmentBackPercent;
                this.FragmentFrontPercent = cells.FragmentFrontPercent;
                this.FragmentTotalPercent = cells.FragmentTotalPercent;

                var unit = await this.machineLoadingUnitsWebService.GetAllAsync();
                //this.TotalDrawers = unit.Count(n => n.IsIntoMachine);
                this.UnitsInCell = unit.Count(n => n.Status == LoadingUnitStatus.InLocation);
                this.UnitsInBay = unit.Count(n => n.Status == LoadingUnitStatus.InBay);
                this.UnitsInElevator = unit.Count(n => n.Status == LoadingUnitStatus.InElevator);
                this.TotalDrawers = this.unitsInCell + this.unitsInBay + this.unitsInElevator;

                var cellsStatistic = await this.machineCellsWebService.GetAllAsync();
                this.TotalCells = cellsStatistic.Count();
                this.BusyCells = cellsStatistic.Count(n => !n.IsFree);
                this.FreeCells = cellsStatistic.Count(n => n.IsFree && n.BlockLevel != BlockLevel.Blocked);
                this.FreeCellsForSupport = cellsStatistic.Count(n => n.IsFree && n.BlockLevel == BlockLevel.None);
                this.FreeCellsOnlySpace = cellsStatistic.Count(n => n.IsFree && n.BlockLevel == BlockLevel.SpaceOnly);
                this.LockedCells = cellsStatistic.Count(n => n.BlockLevel == BlockLevel.Blocked);

                await base.OnDataRefreshAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
