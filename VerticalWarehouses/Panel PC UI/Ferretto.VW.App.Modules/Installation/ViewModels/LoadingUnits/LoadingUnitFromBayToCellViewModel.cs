using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    internal sealed class LoadingUnitFromBayToCellViewModel : BaseCellMovementsViewModel
    {
        #region Constructors

        public LoadingUnitFromBayToCellViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineCellsWebService machineCellsWebService,
            ISensorsService sensorsService,
            IBayManager bayManagerService)
            : base(
                machineLoadingUnitsWebService,
                machineCellsWebService,
                sensorsService,
                bayManagerService)
        {
        }

        #endregion

        #region Methods

        public async Task GetLoadingUnits()
        {
            try
            {
                if (this.LoadingUnitId is null)
                {
                    var lst = await this.MachineLoadingUnitsWebService.GetAllAsync();
                    if (lst.Any())
                    {
                        this.LoadingUnitId = lst.Max(o => o.Id) + 1;
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
                }

                var source = this.GetLoadingUnitSource(this.IsPositionDownSelected);

                if (source == LoadingUnitLocation.NoLocation)
                {
                    this.ShowNotification("Tipo scelta sorgente non valida", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                await this.MachineLoadingUnitsWebService.InsertLoadingUnitAsync(source, null, this.LoadingUnitId.Value);
            }
            catch (Exception ex)
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
            await this.InitializingData();
        }

        private async Task InitializingData()
        {
            await this.GetLoadingUnits();

            this.SelectBayPositionDown();
        }

        #endregion
    }
}
