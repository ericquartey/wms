using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Interfaces;
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

        private bool isBay1Destination;

        private bool isBay1Present;

        private bool isBay2Destination;

        private bool isBay2Present;

        private bool isBay3Destination;

        private bool isBay3Present;

        private IEnumerable<LoadingUnit> loadingUnits;

        private DelegateCommand sendToBay1Command;

        private DelegateCommand sendToBay2Command;

        private DelegateCommand sendToBay3Command;

        private DelegateCommand startToBayCommand;

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

        public bool IsBay1Destination
        {
            get => this.isBay1Destination;
            set => this.SetProperty(ref this.isBay1Destination, value);
        }

        public bool IsBay1Present
        {
            get => this.isBay1Present;
            set => this.SetProperty(ref this.isBay1Present, value);
        }

        public bool IsBay2Destination
        {
            get => this.isBay2Destination;
            set => this.SetProperty(ref this.isBay2Destination, value);
        }

        public bool IsBay2Present
        {
            get => this.isBay2Present;
            set => this.SetProperty(ref this.isBay2Present, value);
        }

        public bool IsBay3Destination
        {
            get => this.isBay3Destination;
            set => this.SetProperty(ref this.isBay3Destination, value);
        }

        public bool IsBay3Present
        {
            get => this.isBay3Present;
            set => this.SetProperty(ref this.isBay3Present, value);
        }

        public ICommand SendToBay1Command =>
            this.sendToBay1Command
            ??
            (this.sendToBay1Command = new DelegateCommand(
                () => this.ChangeDestination(BayNumber.BayOne),
                () => !this.IsBay1Destination && !this.IsExecutingProcedure && !this.IsWaitingForResponse));

        public ICommand SendToBay2Command =>
            this.sendToBay2Command
            ??
            (this.sendToBay2Command = new DelegateCommand(
                () => this.ChangeDestination(BayNumber.BayTwo),
                () => !this.IsBay2Destination && !this.IsExecutingProcedure && !this.IsWaitingForResponse));

        public ICommand SendToBay3Command =>
            this.sendToBay3Command
            ??
            (this.sendToBay3Command = new DelegateCommand(
                () => this.ChangeDestination(BayNumber.BayThree),
                () => !this.IsBay3Destination && !this.IsExecutingProcedure && !this.IsWaitingForResponse));

        public ICommand StartToBayCommand =>
            this.startToBayCommand
            ??
            (this.startToBayCommand = new DelegateCommand(
                async () => await this.StartToBayAsync(),
                () => !this.IsExecutingProcedure &&
                      !this.IsWaitingForResponse &&
                      (!string.IsNullOrEmpty(this.sensorsService.LoadingUnitPositionUpInBayCode) ||
                       !string.IsNullOrEmpty(this.sensorsService.LoadingUnitPositionDownInBayCode))));

        #endregion

        #region Methods

        public async Task GetLoadingUnits()
        {
            try
            {
                var lst = await this.MachineLoadingUnitsWebService.GetAllAsync();
                this.loadingUnits = lst;
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

        public override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.sendToBay1Command?.RaiseCanExecuteChanged();
            this.sendToBay2Command?.RaiseCanExecuteChanged();
            this.sendToBay3Command?.RaiseCanExecuteChanged();
            this.startToBayCommand?.RaiseCanExecuteChanged();
        }

        public async Task StartToBayAsync()
        {
            try
            {
                var source = this.GetLoadingUnitSource(string.IsNullOrEmpty(this.sensorsService.LoadingUnitPositionUpInBayCode));

                if (source == LoadingUnitLocation.NoLocation)
                {
                    this.ShowNotification("Tipo scelta sorgente non valida", Services.Models.NotificationSeverity.Warning);
                    return;
                }
                var loadingUnitCode = !string.IsNullOrEmpty(this.sensorsService.LoadingUnitPositionUpInBayCode) ?
                                      this.sensorsService.LoadingUnitPositionUpInBayCode :
                                      this.sensorsService.LoadingUnitPositionDownInBayCode;

                this.LoadingUnitId = this.loadingUnits.FirstOrDefault(f => f.Code.Equals(loadingUnitCode))?.Id;

                if (this.LoadingUnitId is null)
                {
                    this.ShowNotification("Id cassetto in baia non valido", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                var bay = this.IsBay1Destination ? BayNumber.BayOne : this.IsBay2Destination ? BayNumber.BayTwo : BayNumber.BayThree;
                var destination = this.GetLoadingUnitSourceByDestination(bay, this.IsPositionDownSelected);

                if (destination == LoadingUnitLocation.NoLocation)
                {
                    this.ShowNotification("Tipo scelta destinazione non valida", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsWaitingForResponse = true;

                await this.MachineLoadingUnitsWebService.StartMovingSourceDestinationAsync(source, destination, null, null);
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

        private void ChangeDestination(BayNumber bayDestination)
        {
            if (bayDestination == BayNumber.BayOne)
            {
                this.IsBay1Destination = true;
                this.IsBay2Destination = false;
                this.IsBay3Destination = false;
            }
            else if (bayDestination == BayNumber.BayTwo)
            {
                this.IsBay1Destination = false;
                this.IsBay2Destination = true;
                this.IsBay3Destination = false;
            }
            else
            {
                this.IsBay1Destination = false;
                this.IsBay2Destination = false;
                this.IsBay3Destination = true;
            }

            this.RaiseCanExecuteChanged();
        }

        private async Task InitialinngData()
        {
            await this.GetLoadingUnits();

            this.SelectBayPositionDown();
        }

        private async Task SetDataBays()
        {
            var bays = await this.machineBaysWebService.GetAllAsync();

            this.IsBay1Present = bays.Any(b => b.Number == BayNumber.BayOne);
            this.IsBay2Present = bays.Any(b => b.Number == BayNumber.BayTwo);
            this.IsBay3Present = bays.Any(b => b.Number == BayNumber.BayThree);

            var lst = new List<bool>() { this.IsBay1Present, this.IsBay2Present, this.IsBay3Present };
            if (lst.Count(a => a) == 1)
            {
                if (this.IsBay1Present)
                {
                    this.ChangeDestination(BayNumber.BayOne);
                }

                if (this.IsBay2Present)
                {
                    this.ChangeDestination(BayNumber.BayTwo);
                }

                if (this.IsBay3Present)
                {
                    this.ChangeDestination(BayNumber.BayThree);
                }
            }
        }

        #endregion
    }
}
