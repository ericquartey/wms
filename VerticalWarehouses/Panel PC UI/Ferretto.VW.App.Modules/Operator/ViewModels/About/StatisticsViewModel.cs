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

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private int busyCells;

        private double cellFillPercentage;

        private double fragmentBackPercent;

        private double fragmentFrontPercent;

        private double fragmentTotalPercent;

        private int freeCells;

        private int freeCellsForSupport;

        private int freeCellsOnlySpace;

        private int lockedCells;

        private double maxSolidSpaceBack;

        private double maxSolidSpaceFront;

        private int totalCells;

        private int totalDrawers;

        private MachineStatistics totalStatistics;

        private int unitsInBay;

        private int unitsInCell;

        private int unitsInElevator;

        #endregion

        #region Constructors

        public StatisticsViewModel(
            IMachineCellsWebService machineCellsWebService,
            IMachineIdentityWebService machineIdentityWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService)
            : base()
        {
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
        }

        #endregion

        #region Properties

        public int BusyCells
        {
            get => this.busyCells;
            private set => this.SetProperty(ref this.busyCells, value, this.RaiseCanExecuteChanged);
        }

        public double CellFillPercentage
        {
            get => this.cellFillPercentage;
            set => this.SetProperty(ref this.cellFillPercentage, value, this.RaiseCanExecuteChanged);
        }

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

        public int LockedCells
        {
            get => this.lockedCells;
            private set => this.SetProperty(ref this.lockedCells, value, this.RaiseCanExecuteChanged);
        }

        public double MaxSolidSpaceBack
        {
            get => this.maxSolidSpaceBack;
            set => this.SetProperty(ref this.maxSolidSpaceBack, value, this.RaiseCanExecuteChanged);
        }

        public double MaxSolidSpaceFront
        {
            get => this.maxSolidSpaceFront;
            set => this.SetProperty(ref this.maxSolidSpaceFront, value, this.RaiseCanExecuteChanged);
        }

        public int TotalCells
        {
            get => this.totalCells;
            private set => this.SetProperty(ref this.totalCells, value, this.RaiseCanExecuteChanged);
        }

        public int TotalDrawers
        {
            get => this.totalDrawers;
            private set => this.SetProperty(ref this.totalDrawers, value, this.RaiseCanExecuteChanged);
        }

        public MachineStatistics TotalStatistics
        {
            get => this.totalStatistics;
            set => this.SetProperty(ref this.totalStatistics, value, this.RaiseCanExecuteChanged);
        }

        public int UnitsInBay
        {
            get => this.unitsInBay;
            private set => this.SetProperty(ref this.unitsInBay, value, this.RaiseCanExecuteChanged);
        }

        public int UnitsInCell
        {
            get => this.unitsInCell;
            private set => this.SetProperty(ref this.unitsInCell, value, this.RaiseCanExecuteChanged);
        }

        public int UnitsInElevator
        {
            get => this.unitsInElevator;
            private set => this.SetProperty(ref this.unitsInElevator, value, this.RaiseCanExecuteChanged);
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

                foreach (var spaceSide in cells.MaxSolidSpace)
                {
                    if (spaceSide.Key == WarehouseSide.Front)
                    {
                        this.MaxSolidSpaceFront = spaceSide.Value;
                    }
                    else if (spaceSide.Key == WarehouseSide.Back)
                    {
                        this.MaxSolidSpaceBack = spaceSide.Value;
                    }
                }

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
                this.CellFillPercentage = (double)(this.BusyCells + this.LockedCells) / this.TotalCells * 100;

                this.TotalStatistics = await this.machineIdentityWebService.GetStatisticsAsync();
                //this.TotalStatistics = new MachineStatistics();

                //this.TotalStatistics.AutomaticTimePercentage = machineStatistics.Select(s => s.AutomaticTimePercentage).Sum();
                //this.TotalStatistics.WeightCapacityPercentage = machineStatistics.Select(s => s.WeightCapacityPercentage).Sum();
                //this.TotalStatistics.UsageTimePercentage = machineStatistics.Select(s => s.UsageTimePercentage).Sum();
                //this.TotalStatistics.AreaFillPercentage = machineStatistics.Select(s => s.AreaFillPercentage).Sum();

                this.RaisePropertyChanged(nameof(this.TotalStatistics));

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
