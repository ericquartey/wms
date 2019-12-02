using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    internal sealed class LoadingUnitFromBayToBayViewModel : BaseMovementsViewModel
    {
        #region Fields

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly ISensorsService sensorsService;

        private bool isBay1Present;

        private bool isBay2Present;

        private bool isBay3Present;

        private DelegateCommand sendToBay1Command;

        private DelegateCommand sendToBay2Command;

        private DelegateCommand sendToBay3Command;

        #endregion

        #region Constructors

        public LoadingUnitFromBayToBayViewModel(
            IMachineBaysWebService machineBaysWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            ISensorsService sensorsService,
            IBayManager bayManagerService)
            : base(
                machineLoadingUnitsWebService,
                bayManagerService)
        {
            this.machineBaysWebService = machineBaysWebService ?? throw new System.ArgumentNullException(nameof(machineBaysWebService));
            this.sensorsService = sensorsService ?? throw new ArgumentNullException(nameof(sensorsService));
        }

        #endregion

        #region Properties

        public bool IsBay1Present
        {
            get => this.isBay1Present;
            set => this.SetProperty(ref this.isBay1Present, value);
        }

        public bool IsBay2Present
        {
            get => this.isBay2Present;
            set => this.SetProperty(ref this.isBay2Present, value);
        }

        public bool IsBay3Present
        {
            get => this.isBay3Present;
            set => this.SetProperty(ref this.isBay3Present, value);
        }

        public ICommand SendToBay1Command =>
            this.sendToBay1Command
            ??
            (this.sendToBay1Command = new DelegateCommand(async () => await this.StartToBayAsync(1)));

        public ICommand SendToBay2Command =>
            this.sendToBay2Command
            ??
            (this.sendToBay2Command = new DelegateCommand(async () => await this.StartToBayAsync(2)));

        public ICommand SendToBay3Command =>
            this.sendToBay3Command
            ??
            (this.sendToBay3Command = new DelegateCommand(async () => await this.StartToBayAsync(3)));

        #endregion

        #region Methods

        public async Task GetLoadingUnits()
        {
            try
            {
                if (this.LoadingUnitId is null)
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

            await this.InitialinngData();

            await this.SetDataBays();
        }

        public async Task StartToBayAsync(int bayDestination)
        {
            try
            {
                if (!this.IsLoadingUnitIdValid)
                {
                    this.ShowNotification("Id cassetto inserito non valido", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                var destination = this.GetLoadingUnitSource();

                if (destination == LoadingUnitLocation.NoLocation)
                {
                    this.ShowNotification("Tipo scelta sorgente non valida", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsWaitingForResponse = true;

                await this.MachineLoadingUnitsWebService.StartMovingLoadingUnitToBayAsync(this.LoadingUnitId.Value, destination);
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

            this.GetLoadingUnits().ConfigureAwait(false);
        }

        private async Task InitialinngData()
        {
            await this.GetLoadingUnits();

            this.SelectBayPositionDown();
        }

        private async Task SetDataBays()
        {
            var bays = await this.machineBaysWebService.GetAllAsync();

            this.IsBay1Present = bays.Any(b => b.Number == BayNumber.BayOne && b.Number != this.Bay.Number);
            this.IsBay2Present = bays.Any(b => b.Number == BayNumber.BayTwo && b.Number != this.Bay.Number);
            this.IsBay3Present = bays.Any(b => b.Number == BayNumber.BayThree && b.Number != this.Bay.Number);
        }

        #endregion
    }
}
