﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Ferretto.VW.App.Resources;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class LoadingUnitFromCellToBayViewModel : BaseMovementsViewModel
    {
        #region Fields

        private readonly IMachineBaysWebService machineBaysWebService;

        private DelegateCommand confirmEjectLoadingUnitCommand;

        private bool isEjectLoadingUnitConfirmationEnabled;

        #endregion

        #region Constructors

        public LoadingUnitFromCellToBayViewModel(
            IMachineBaysWebService machineBaysWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineModeWebService machineModeWebService,
            IBayManager bayManagerService)
            : base(
                machineLoadingUnitsWebService,
                machineModeWebService,
                bayManagerService)
        {
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
        }

        #endregion

        #region Properties

        public ICommand ConfirmEjectLoadingUnitCommand =>
        this.confirmEjectLoadingUnitCommand
        ??
        (this.confirmEjectLoadingUnitCommand = new DelegateCommand(
            async () => await this.ConfirmEjectLoadingUnit(),
                        this.CanConfirmEjectLoadingUnit));

        #endregion

        #region Methods

        public override bool CanSelectBayPositionDown()
        {
            return !this.IsExecutingProcedure &&
                   this.IsPositionUpSelected &&
                   (this.MachineStatus.LoadingUnitPositionUpInBay is null ||
                    this.MachineStatus.LoadingUnitPositionDownInBay is null);
        }

        public override bool CanSelectBayPositionUp()
        {
            return !this.IsExecutingProcedure &&
                   !this.IsPositionUpSelected &&
                   (this.MachineStatus.LoadingUnitPositionUpInBay is null ||
                    this.MachineStatus.LoadingUnitPositionDownInBay is null);
        }

        public override bool CanStart()
        {
            return base.CanStart() &&
                   this.LoadingUnitId.HasValue &&
                   this.MachineModeService.MachineMode == MachineMode.LoadUnitOperations &&
                   this.MachineService.Loadunits.Any(f => f.Id == this.LoadingUnitId && f.Status == LoadingUnitStatus.InLocation) &&
                   (this.MachineStatus.LoadingUnitPositionUpInBay is null ||
                    (!this.MachineService.HasCarousel && this.MachineStatus.LoadingUnitPositionDownInBay is null));
        }

        public override async Task OnAppearedAsync()
        {
            this.LoadingUnitId = null;

            await base.OnAppearedAsync();
        }

        public override async Task StartAsync()
        {
            try
            {
                if (!this.IsLoadingUnitIdValid)
                {
                    this.ShowNotification(InstallationApp.InvalidEnteredDrawerId, Services.Models.NotificationSeverity.Warning);
                    return;
                }

                var destination = this.GetLoadingUnitSource(this.IsPositionDownSelected);

                if (destination == LoadingUnitLocation.NoLocation)
                {
                    this.ShowNotification(InstallationApp.InvalidSourceChoiceType, Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsWaitingForResponse = true;

                //await this.MachineLoadingUnitsWebService.EjectLoadingUnitAsync(destination, this.LoadingUnitId.Value);
                await this.MachineLoadingUnitsWebService.StartMovingLoadingUnitToBayAsync(this.LoadingUnitId.Value, destination);
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

        protected override void Ended()
        {
            base.Ended();

            this.isEjectLoadingUnitConfirmationEnabled = true;

            this.RaiseCanExecuteChanged();
        }

        protected override async Task OnDataRefreshAsync()
        {
            await this.SensorsService.RefreshAsync(true);

            if (this.MachineService.Bay.IsDouble || this.MachineService.BayFirstPositionIsUpper || this.MachineService.HasCarousel)
            {
                this.SelectBayPositionUp();
            }
            else
            {
                this.SelectBayPositionDown();
            }
        }

        protected override void OnWaitResume()
        {
            this.isEjectLoadingUnitConfirmationEnabled = true;

            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.confirmEjectLoadingUnitCommand?.RaiseCanExecuteChanged();
        }

        private bool CanConfirmEjectLoadingUnit()
        {
            return !this.IsMoving &&
                   ((this.MachineStatus.LoadingUnitPositionUpInBay != null && this.IsPositionUpSelected) ||
                    (this.MachineStatus.LoadingUnitPositionDownInBay != null && this.IsPositionDownSelected));
        }

        private async Task ConfirmEjectLoadingUnit()
        {
            try
            {
                int lu = 0;

                if (this.IsPositionUpSelected)
                {
                    lu = this.MachineStatus.LoadingUnitPositionUpInBay.Id;
                }
                else
                {
                    lu = this.MachineStatus.LoadingUnitPositionDownInBay.Id;
                }

                await this.machineBaysWebService.RemoveLoadUnitAsync(lu);

                var refreshTask = this.SensorsService.RefreshAsync(true);

                var updateTask = this.MachineService.OnUpdateServiceAsync();

                this.ShowNotification(string.Format(InstallationApp.DrawerIdExtracted, lu), Services.Models.NotificationSeverity.Warning);

                await refreshTask;

                await updateTask;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(e);
            }
        }

        #endregion
    }
}
