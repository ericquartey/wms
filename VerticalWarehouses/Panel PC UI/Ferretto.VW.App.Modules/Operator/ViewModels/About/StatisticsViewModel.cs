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
            protected set => this.SetProperty(ref this.totalDrawers, value, this.RaiseCanExecuteChanged);
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
                this.TotalDrawers = unit.Count(n => n.IsIntoMachine);

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
