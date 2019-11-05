using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal abstract class BaseCellsHeightCheckViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private IEnumerable<Cell> cells;

        private double? currentPosition;

        private bool isWaitingForResponse;

        private Cell selectedCell;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public BaseCellsHeightCheckViewModel(
            IMachineCellsWebService machineCellsWebService,
            IMachineElevatorWebService machineElevatorWebService)
            : base(Services.PresentationMode.Installer)
        {
            this.MachineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.MachineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));

            this.InitializeNavigationMenu();
        }

        #endregion

        #region Properties

        public double? CurrentPosition
        {
            get => this.currentPosition;
            private set => this.SetProperty(ref this.currentPosition, value);
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value))
                {
                    if (this.isWaitingForResponse)
                    {
                        this.ClearNotifications();
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

        public Cell SelectedCell
        {
            get => this.selectedCell;
            protected set
            {
                if (this.SetProperty(ref this.selectedCell, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        protected IEnumerable<Cell> Cells
        {
            get => this.cells;
            private set
            {
                if (this.SetProperty(ref this.cells, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        protected IMachineCellsWebService MachineCellsWebService { get; }

        protected IMachineElevatorWebService MachineElevatorWebService { get; }

        protected PositioningProcedure ProcedureParameters { get; private set; }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            /*
             * Avoid unsubscribing in case of navigation to error page.
             * We may need to review this behaviour.
             *
            this.subscriptionToken?.Dispose();
            this.subscriptionToken = null;
            */
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.subscriptionToken = this.subscriptionToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        this.OnCurrentPositionChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data?.CurrentPosition != null);

            await this.RetrieveCurrentPositionAsync();

            await this.RetrieveCellsAsync();

            await this.RetrieveProcedureParametersAsync();
        }

        protected virtual void OnCurrentPositionChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            this.CurrentPosition = message.Data.CurrentPosition;
        }

        protected abstract void RaiseCanExecuteChanged();

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
                this.Cells = await this.MachineCellsWebService.GetAllAsync();
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
                this.CurrentPosition = await this.MachineElevatorWebService.GetVerticalPositionAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task RetrieveProcedureParametersAsync()
        {
            try
            {
                this.ProcedureParameters = await this.MachineCellsWebService.GetHeightCheckProcedureParametersAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
