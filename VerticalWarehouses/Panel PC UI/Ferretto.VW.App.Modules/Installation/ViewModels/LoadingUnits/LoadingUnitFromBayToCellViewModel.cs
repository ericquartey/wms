﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.CodeParser;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class LoadingUnitFromBayToCellViewModel : BaseCellMovementsViewModel
    {
        #region Fields

        private bool isEnabledEditing;

        private LoadingUnit selectedLU;

        #endregion

        #region Constructors

        public LoadingUnitFromBayToCellViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineModeWebService machineModeWebService,
            IMachineCellsWebService machineCellsWebService,
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

        #region Properties

        public bool HasExternalDouble => this.MachineService.Bay.IsExternal && this.MachineService.Bay.IsDouble;

        public bool IsEnabledEditing
        {
            get => this.isEnabledEditing;
            set => this.SetProperty(ref this.isEnabledEditing, value && !this.IsMoving);
        }

        public LoadingUnit SelectedLU
        {
            get => this.selectedLU;
            set => this.SetProperty(ref this.selectedLU, value);
        }

        #endregion

        #region Methods

        public override bool CanStart()
        {
            if (this.LoadingUnitId.HasValue &&
                (this.selectedLU is null || this.selectedLU.Id != this.LoadingUnitId)
                )
            {
                this.UpdateDrawerInfo();
            }
            // Conditions to check
            // Load from InBay only if bay is NOT double and Bay first position is upper
            var checkP1 = this.SensorsService.IsLoadingUnitInBay && (!this.MachineService.Bay.IsDouble) && this.MachineService.BayFirstPositionIsUpper;
            // Load from InBay only if bay is carousel
            var checkP2 = this.SensorsService.IsLoadingUnitInBay && (this.MachineService.HasCarousel);
            // Load from InBay only if bay is double and Bay selected position is upper
            var checkP3 = this.SensorsService.IsLoadingUnitInBay && this.MachineService.Bay.IsDouble && this.IsPositionUpSelected && !this.MachineService.Bay.IsExternal;
            // Load from MiddleBottomBay only if bay is double and Bay selected position is lower
            var checkP4 = this.SensorsService.IsLoadingUnitInMiddleBottomBay && this.MachineService.Bay.IsDouble && this.IsPositionDownSelected && !this.MachineService.Bay.IsExternal;
            // Load from MiddleBottomBay only if bay is not carousel, bay is not double and Bay first position is NOT upper
            var checkP5 = this.SensorsService.IsLoadingUnitInMiddleBottomBay &&
                !this.MachineService.HasCarousel &&
                !this.MachineService.Bay.IsDouble &&
                !this.MachineService.BayFirstPositionIsUpper;
            // Load from Top bay only if bay is external double
            var checkP6 = (this.SensorsService.BEDInternalBayTop || this.SensorsService.BEDExternalBayTop) &&
                this.IsPositionUpSelected &&
                this.MachineService.Bay.IsDouble &&
                this.MachineService.Bay.IsExternal;
            // Load from bottom bay only if bay is external double
            var checkP7 = (this.SensorsService.BEDInternalBayBottom || this.SensorsService.BEDExternalBayBottom) &&
                this.IsPositionDownSelected &&
                this.MachineService.Bay.IsDouble &&
                this.MachineService.Bay.IsExternal;

            switch (this.MachineService.BayNumber)
            {
                case BayNumber.BayOne:
                default:
                    return base.CanStart() &&
                   !this.IsMoving &&
                   this.MachineModeService.MachineMode == MachineMode.Manual &&
                   (checkP1 || checkP2 || checkP3 || checkP4 || checkP5 || checkP6 || checkP7) &&
                   this.LoadingUnitId.HasValue &&
                   !this.MachineService.Loadunits.DrawerInLocationById(this.LoadingUnitId.Value);

                case BayNumber.BayTwo:
                    return base.CanStart() &&
                   !this.IsMoving &&
                   this.MachineModeService.MachineMode == MachineMode.Manual2 &&
                   (checkP1 || checkP2 || checkP3 || checkP4 || checkP5 || checkP6 || checkP7) &&
                   this.LoadingUnitId.HasValue &&
                   !this.MachineService.Loadunits.DrawerInLocationById(this.LoadingUnitId.Value);

                case BayNumber.BayThree:
                    return base.CanStart() &&
                   !this.IsMoving &&
                   this.MachineModeService.MachineMode == MachineMode.Manual3 &&
                   (checkP1 || checkP2 || checkP3 || checkP4 || checkP5 || checkP6 || checkP7) &&
                   this.LoadingUnitId.HasValue &&
                   !this.MachineService.Loadunits.DrawerInLocationById(this.LoadingUnitId.Value);
            }
        }

        public async Task GetLoadingUnits()
        {
            try
            {
                if (this.LoadingUnitId is null)
                {
                    var lst = await this.MachineLoadingUnitsWebService.GetAllAsync();
                    if (lst.Any())
                    {
                        this.LoadingUnitId = lst.Where(x => x.Status == LoadingUnitStatus.Undefined).FirstOrDefault()?.Id ?? (lst.Max(o => o.Id) + 1);
                    }
                    else
                    {
                        this.LoadingUnitId = null;
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
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
                this.IsWaitingForResponse = true;

                if (!this.IsLoadingUnitIdValid)
                {
                    await this.MachineLoadingUnitsWebService.InsertLoadingUnitOnlyDbAsync(this.LoadingUnitId.Value);
                    if (this.SelectedLU != null)
                    {
                        this.SelectedLU.Id = this.LoadingUnitId.Value;
                    }
                }
                if (this.isEnabledEditing && this.SelectedLU != null)
                {
                    await this.MachineLoadingUnitsWebService.SaveLoadUnitAsync(this.SelectedLU);
                }

                var source = this.GetLoadingUnitSource(this.IsPositionDownSelected);

                if (source == LoadingUnitLocation.NoLocation)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.InvalidSourceChoiceType"), Services.Models.NotificationSeverity.Warning);
                    return;
                }

                await this.MachineLoadingUnitsWebService.InsertLoadingUnitAsync(source, null, this.LoadingUnitId.Value);
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

            this.RetrieveCellsAsync().ConfigureAwait(false);

            this.GetLoadingUnits().ConfigureAwait(false);
        }

        protected override async Task OnDataRefreshAsync()
        {
            await this.SensorsService.RefreshAsync(true);

            await this.InitializingData();
        }

        private async Task InitializingData()
        {
            await this.GetLoadingUnits();

            if (this.MachineService.Bay.Positions.Any(p => p.IsUpper && !p.IsBlocked) &&
                ((this.MachineService.Bay.IsDouble && !this.MachineService.Bay.IsExternal) || this.MachineService.BayFirstPositionIsUpper || this.MachineService.HasCarousel)
                )
            {
                this.SelectBayPositionUp();
            }
            else
            {
                this.SelectBayPositionDown();
            }

            if (this.MachineStatus.LoadingUnitPositionDownInBay != null)
            {
                this.LoadingUnitId = this.MachineStatus.LoadingUnitPositionDownInBay.Id;
            }

            if (this.MachineStatus.LoadingUnitPositionUpInBay != null)
            {
                this.LoadingUnitId = this.MachineStatus.LoadingUnitPositionUpInBay.Id;
            }
            await this.UpdateDrawerInfo();
        }

        private async Task UpdateDrawerInfo()
        {
            var lst = await this.MachineLoadingUnitsWebService.GetAllAsync();

            if (!lst.Any())
            {
                this.isEnabledEditing = true;
                this.RaisePropertyChanged(nameof(this.IsEnabledEditing));
                return;
            }

            if (this.LoadingUnitId.HasValue && lst.Any(i => i.Id == this.LoadingUnitId))
            {
                this.isEnabledEditing = false;
                this.selectedLU = lst.FirstOrDefault(i => i.Id == this.LoadingUnitId);
            }
            else
            {
                this.isEnabledEditing = true;
                var maxLU = lst.OrderByDescending(S => S.Id).FirstOrDefault();
                this.selectedLU = new LoadingUnit();
                if (maxLU != null)
                {
                    this.selectedLU.Tare = maxLU.Tare;
                    this.selectedLU.Height = maxLU.Height;
                    this.selectedLU.MaxNetWeight = maxLU.MaxNetWeight;
                    this.selectedLU.NetWeight = maxLU.NetWeight;
                    this.selectedLU.GrossWeight = maxLU.GrossWeight;
                    this.selectedLU.Id = this.LoadingUnitId.Value;
                }
            }

            this.RaisePropertyChanged(nameof(this.SelectedLU));
            this.RaisePropertyChanged(nameof(this.IsEnabledEditing));
            this.RaisePropertyChanged(nameof(this.HasExternalDouble));
        }

        #endregion
    }
}
