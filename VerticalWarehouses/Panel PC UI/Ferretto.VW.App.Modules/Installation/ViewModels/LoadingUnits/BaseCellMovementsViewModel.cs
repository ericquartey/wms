using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Regions;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class BaseCellMovementsViewModel : BaseMovementsViewModel
    {
        #region Fields

        private readonly IMachineCellsWebService machineCellsWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IMachineElevatorWebService>();

        private readonly ISensorsService sensorsService;

        private AxisBounds axisBounds;

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
                int? old = this.destinationCellId;
                if (this.SetProperty(ref this.destinationCellId, value))
                {
                    if (this.axisBounds != null)
                    {
                        if (this.IsCellFree)
                        {
                            this.RaiseCanExecuteChanged();
                        }
                        else
                        {
                            bool increment = !old.HasValue || old < this.destinationCellId;
                            if (increment)
                            {
                                var l = this.Cells.Where(w => w.Position > this.axisBounds?.Lower &&
                                                              w.Position < this.axisBounds?.Upper &&
                                                              w.Status == CellStatus.Free &&
                                                              w.Id > (old ?? 0));
                                if (l.Any())
                                {
                                    this.DestinationCellId = l.Min(o => o.Id);
                                }
                            }
                            else
                            {
                                var l = this.Cells.Where(w => w.Position > this.axisBounds?.Lower &&
                                                              w.Position < this.axisBounds?.Upper &&
                                                              w.Status == CellStatus.Free &&
                                                              old.HasValue &&
                                                              w.Id < old.Value);
                                if (l.Any())
                                {
                                    this.DestinationCellId = l.Max(o => o.Id);
                                }
                                else
                                {
                                    this.DestinationCellId = null;
                                }
                            }
                        }
                    }
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

                var cellFound = this.cells.FirstOrDefault(l =>
                                                          !(this.axisBounds is null) &&
                                                          l.Position > this.axisBounds.Lower &&
                                                          l.Position < this.axisBounds.Upper &&
                                                          l.Id == this.destinationCellId.Value);
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

        protected async Task RetrieveCellsAsync()
        {
            try
            {
                this.axisBounds = await this.machineElevatorWebService.GetVerticalBoundsAsync();

                this.Cells = await this.machineCellsWebService.GetAllAsync();
                if (this.DestinationCellId is null)
                {
                    if (this.Cells.Count() > 0)
                    {
                        this.DestinationCellId = this.Cells.Where(w => w.Status == CellStatus.Free).Min(o => o.Id);
                    }
                    else
                    {
                        this.DestinationCellId = null;
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
