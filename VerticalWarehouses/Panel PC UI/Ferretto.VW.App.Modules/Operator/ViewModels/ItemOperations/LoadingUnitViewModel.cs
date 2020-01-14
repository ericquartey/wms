using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class LoadingUnitViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly WMS.Data.WebAPI.Contracts.ILoadingUnitsWmsWebService loadingUnitsWmsWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private Bay bay;

        private IEnumerable<TrayControlCompartment> compartments;

        private DelegateCommand confirmOperationCommand;

        private double loadingUnitDepth;

        private double loadingUnitWidth;

        #endregion

        #region Constructors

        public LoadingUnitViewModel(
            IBayManager bayManager,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            WMS.Data.WebAPI.Contracts.ILoadingUnitsWmsWebService loadingUnitsWmsWebService)
            : base(PresentationMode.Operator)
        {
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.loadingUnitsWmsWebService = loadingUnitsWmsWebService;
        }

        #endregion

        #region Properties

        public IEnumerable<TrayControlCompartment> Compartments
        {
            get => this.compartments;
            set => this.SetProperty(ref this.compartments, value);
        }

        public ICommand ConfirmOperationCommand =>
            this.confirmOperationCommand
            ??
            (this.confirmOperationCommand = new DelegateCommand(
                async () => await this.ConfirmOperationAsync(),
                this.CanConfirmOperation));

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsBaySideBack => this.bay?.Side is WarehouseSide.Back;

        public LoadingUnit LoadingUnit { get; set; }

        public double LoadingUnitDepth
        {
            get => this.loadingUnitDepth;
            set => this.SetProperty(ref this.loadingUnitDepth, value, this.RaiseCanExecuteChanged);
        }

        public double LoadingUnitWidth
        {
            get => this.loadingUnitWidth;
            set => this.SetProperty(ref this.loadingUnitWidth, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public virtual bool CanConfirmOperation()
        {
            return
                !this.IsWaitingForResponse
                &&
                this.LoadingUnit != null;
        }

        public async Task ConfirmOperationAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.machineLoadingUnitsWebService.RemoveFromBayAsync(this.LoadingUnit.Id);

                this.NavigationService.GoBack();
            }
            catch (MasWebApiException ex)
            {
                this.ShowNotification(ex);
            }
        }

        public async override Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.bay = await this.bayManager.GetBayAsync();

            if (this.Data is int loadingUnitId)
            {
                this.LoadingUnit = this.MachineService.Loadunits.SingleOrDefault(l => l.Id == loadingUnitId);
                try
                {
                    var wmsLoadingUnit = await this.loadingUnitsWmsWebService.GetByIdAsync(loadingUnitId);
                    this.LoadingUnitWidth = wmsLoadingUnit.Width;
                    this.LoadingUnitDepth = wmsLoadingUnit.Depth;

                    var wmsCompartments = await this.loadingUnitsWmsWebService.GetCompartmentsAsync(loadingUnitId);
                    this.Compartments = MapCompartments(wmsCompartments);
                }
                catch (WMS.Data.WebAPI.Contracts.WmsWebApiException)
                {
                    // do nothing: details will not be shown
                }

                this.RaisePropertyChanged(nameof(this.LoadingUnit));
                this.confirmOperationCommand?.RaiseCanExecuteChanged();
            }
        }

        private static IEnumerable<TrayControlCompartment> MapCompartments(
            IEnumerable<WMS.Data.WebAPI.Contracts.CompartmentDetails> compartmentsFromMission)
        {
            return compartmentsFromMission
                .Select(c => new TrayControlCompartment
                {
                    Depth = c.Depth,
                    Id = c.Id,
                    Width = c.Width,
                    XPosition = c.XPosition,
                    YPosition = c.YPosition,
                });
        }

        #endregion
    }
}
