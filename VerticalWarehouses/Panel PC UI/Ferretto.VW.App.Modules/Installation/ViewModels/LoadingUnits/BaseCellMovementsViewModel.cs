using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class BaseCellMovementsViewModel : BaseMovementsViewModel
    {
        #region Fields

        private readonly IMachineCellsWebService machineCellsWebService;

        private readonly ISensorsService sensorsService;

        private IEnumerable<Cell> cells;

        private int? destinationCellId;

        #endregion

        #region Constructors

        public BaseCellMovementsViewModel(
                    IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
                    IMachineCellsWebService machineCellsWebService,
                    Controls.Interfaces.ISensorsService sensorsService,
                    IBayManager bayManagerService)
            : base(
                machineLoadingUnitsWebService,
                bayManagerService)
        {
            if (machineCellsWebService is null)
            {
                throw new ArgumentNullException(nameof(machineCellsWebService));
            }

            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.sensorsService = sensorsService ?? throw new ArgumentNullException(nameof(sensorsService));
        }

        #endregion

        #region Properties

        public int? DestinationCellId
        {
            get => this.destinationCellId;
            set
            {
                if (this.SetProperty(ref this.destinationCellId, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsCellFree
        {
            get
            {
                if (!this.destinationCellId.HasValue)
                {
                    return false;
                }

                var cellFound = this.cells.FirstOrDefault(l => l.Id == this.destinationCellId.Value);
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
                if (!this.destinationCellId.HasValue)
                {
                    return false;
                }

                return this.cells.Any(l => l.Id == this.destinationCellId.Value);
            }
        }

        public bool IsLoadingUnitInBay => this.sensorsService.IsLoadingUnitInBay;

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

        #endregion

        #region Methods

        public async override Task OnAppearedAsync()
        {
            await this.RetrieveCellsAsync();

            await base.OnAppearedAsync();
        }

        private async Task RetrieveCellsAsync()
        {
            try
            {
                this.Cells = await this.machineCellsWebService.GetAllAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
