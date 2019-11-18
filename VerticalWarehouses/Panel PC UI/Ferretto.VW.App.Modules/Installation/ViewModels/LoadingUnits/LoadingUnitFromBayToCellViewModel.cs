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
            Controls.Interfaces.ISensorsService sensorsService,
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
                var lst = await this.MachineLoadingUnitsWebService.GetAllAsync();
                if (lst.Count() > 0)
                {
                    this.LoadingUnitId = lst.Max(o => o.Id) + 1;
                }
                else
                {
                    this.LoadingUnitId = null;
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

            await this.GetLoadingUnits();

            this.SelectBayPositionDown();
        }

        public override async Task StartAsync()
        {
            try
            {
                if (!this.IsLoadingUnitIdValid)
                {
                    await this.MachineLoadingUnitsWebService.InsertLoadingUnitOnlyDbAsync(this.LoadingUnitId.Value);
                }

                if (!this.IsCellIdValid)
                {
                    this.ShowNotification("Id cella inserito non valido", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                if (!this.IsCellFree)
                {
                    this.ShowNotification("la cella inserita non è libera", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                var source = this.GetLoadingUnitSource();

                if (source == LoadingUnitLocation.NoLocation)
                {
                    this.ShowNotification("Tipo scelta sorgente non valida", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsWaitingForResponse = true;

                await this.MachineLoadingUnitsWebService.InsertLoadingUnitAsync(source, this.DestinationCellId.Value, this.LoadingUnitId.Value);

                await this.RetrieveCellsAsync();

                await this.GetLoadingUnits();
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

        #endregion
    }
}
