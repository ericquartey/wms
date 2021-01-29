using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class BaseCellMovementsViewModel : BaseMovementsViewModel
    {
        #region Fields

        private readonly IMachineCellsWebService machineCellsWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IMachineElevatorWebService>();

        private readonly ISensorsService sensorsService;

        private AxisBounds axisBounds;

        private int? destinationCellId;

        #endregion

        #region Constructors

        public BaseCellMovementsViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineCellsWebService machineCellsWebService,
            IMachineModeWebService machineModeWebService,
            ISensorsService sensorsService,
            IBayManager bayManagerService,
            IMachineExternalBayWebService machineExternalBayWebService)
            : base(
                machineLoadingUnitsWebService,
                machineModeWebService,
                bayManagerService,
                machineExternalBayWebService)
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

        private IEnumerable<Cell> Cells => this.MachineService.Cells;

        public int? DestinationCellId
        {
            get => this.destinationCellId;
            set
            {
                var old = this.destinationCellId;
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
                            var increment = !old.HasValue || old < this.destinationCellId;
                            if (increment)
                            {
                                var l = this.Cells.Where(w => w.Position > this.axisBounds?.Lower &&
                                                              w.Position < this.axisBounds?.Upper &&
                                                              w.IsFree &&
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
                                                              w.IsFree &&
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

                var cellFound = this.Cells.FirstOrDefault(l =>
                                                          !(this.axisBounds is null) &&
                                                          l.Position > this.axisBounds.Lower &&
                                                          l.Position < this.axisBounds.Upper &&
                                                          l.Id == this.destinationCellId.Value);
                if (!(cellFound is null))
                {
                    return cellFound.IsFree;
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

                return this.Cells.Any(l => l.Id == this.destinationCellId.Value);
            }
        }

        public bool IsLoadingUnitInBay => this.sensorsService.IsLoadingUnitInBay;

        #endregion

        #region Methods

        protected async Task RetrieveCellsAsync()
        {
            try
            {
                this.axisBounds = await this.machineElevatorWebService.GetVerticalBoundsAsync();

                if (this.DestinationCellId is null)
                {
                    this.DestinationCellId = this.Cells.Any() ? this.Cells.Where(w => w.IsFree).Min(o => o.Id) : (int?)null;
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
