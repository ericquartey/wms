using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class LoadingUnitFromCellToCellViewModel : BaseCellMovementsViewModel
    {
        #region Constructors

        public LoadingUnitFromCellToCellViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineCellsWebService machineCellsWebService,
            IMachineModeWebService machineModeWebService,
            ISensorsService sensorsService,
            IBayManager bayManagerService,
            IMachineExternalBayWebService machineExternalBayWebService)
            : base(
                machineLoadingUnitsWebService,
                machineCellsWebService,
                machineModeWebService,
                sensorsService,
                bayManagerService,
                machineExternalBayWebService)
        {
        }

        #endregion

        #region Methods

        public override bool CanStart()
        {
            switch (this.MachineService.BayNumber)
            {
                case BayNumber.BayOne:
                default:
                    return base.CanStart() &&
                        this.MachineModeService.MachineMode == MachineMode.Manual;

                case BayNumber.BayTwo:
                    return base.CanStart() &&
                        this.MachineModeService.MachineMode == MachineMode.Manual2;

                case BayNumber.BayThree:
                    return base.CanStart() &&
                        this.MachineModeService.MachineMode == MachineMode.Manual3;
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
        }

        public override async Task StartAsync()
        {
            try
            {
                if (!this.IsLoadingUnitIdValid)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.InvalidEnteredDrawerId"), Services.Models.NotificationSeverity.Warning);
                    return;
                }

                if (this.LoadingUnitCellId == null)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.InvalidCellIdOnInsertedDrawer"), Services.Models.NotificationSeverity.Warning);
                    return;
                }

                if (!this.IsCellIdValid)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.InvalidCellIdEntered"), Services.Models.NotificationSeverity.Warning);
                    return;
                }

                if (!this.IsCellFree)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.CellInsertedNotFree"), Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsWaitingForResponse = true;

                await this.MachineLoadingUnitsWebService.StartMovingSourceDestinationAsync(LoadingUnitLocation.Cell, LoadingUnitLocation.Cell, this.LoadingUnitCellId, this.DestinationCellId);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        protected override async Task OnDataRefreshAsync()
        {
            await this.SensorsService.RefreshAsync(true);

            await this.RetrieveCellsAsync();
        }

        #endregion
    }
}
