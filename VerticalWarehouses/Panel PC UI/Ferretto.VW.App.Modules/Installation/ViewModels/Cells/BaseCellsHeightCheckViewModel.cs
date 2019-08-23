using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public abstract class BaseCellsHeightCheckViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineCellsService machineCellsService;

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private IEnumerable<Cell> cells;

        private decimal? currentPosition;

        private Cell selectedCell;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public BaseCellsHeightCheckViewModel(
            IMachineCellsService machineCellsService,
            IMachineElevatorService machineElevatorService)
            : base(Services.PresentationMode.Installer)
        {
            if (machineCellsService is null)
            {
                throw new System.ArgumentNullException(nameof(machineCellsService));
            }

            if (machineElevatorService is null)
            {
                throw new ArgumentNullException(nameof(machineElevatorService));
            }

            this.machineCellsService = machineCellsService;
            this.MachineElevatorService = machineElevatorService;

            this.InitializeNavigationMenu();
        }

        #endregion

        #region Properties

        protected IMachineElevatorService MachineElevatorService { get; }

        public IEnumerable<Cell> Cells
        {
            get => this.cells;
            private set => this.SetProperty(ref this.cells, value);
        }

        public decimal? CurrentPosition
        {
            get => this.currentPosition;
            private set => this.SetProperty(ref this.currentPosition, value);
        }

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

        public Cell SelectedCell
        {
            get => this.selectedCell;
            set
            {
                if (this.SetProperty(ref this.selectedCell, value))
                {
                    this.RefreshCanExecuteCommands();
                }
            }
        }

        #endregion

        #region Methods

        private void InitializeNavigationMenu()
        {
            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.CellsHeightCheck.STEP1,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Step1,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.CellsHeightCheck.STEP2,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Step2,
                    trackCurrentView: false));
        }

        private async Task RetrieveCellsAsync()
        {
            try
            {
                this.Cells = await this.machineCellsService.GetAllAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task RetrieveCurrentPositionAsync()
        {
            try
            {
                this.CurrentPosition = await this.MachineElevatorService.GetVerticalPositionAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected abstract void RefreshCanExecuteCommands();

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.subscriptionToken = this.EventAggregator
                .GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(
                    message => this.CurrentPosition = message?.Data?.CurrentPosition,
                    ThreadOption.UIThread,
                    false);

            await this.RetrieveCurrentPositionAsync();

            await this.RetrieveCellsAsync();
        }

        #endregion
    }
}
