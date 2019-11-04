using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public enum ProfileHeightCheckStep
    {
        /// <summary>
        /// Step 1
        /// </summary>
        Initialize,

        /// <summary>
        /// Step 2
        /// </summary>
        ElevatorPosition,

        /// <summary>
        /// Step 3
        /// </summary>
        DrawerPosition,

        /// <summary>
        /// Step 4
        /// </summary>
        ShapePosition,

        /// <summary>
        /// Step 5
        /// </summary>
        TaraturaCatena,

        /// <summary>
        /// Step 6
        /// </summary>
        ResultCheck,
    }

    internal abstract class BaseProfileHeightCheckViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        protected ProfileHeightCheckStep currentStep;

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineModeService machineModeService;

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private readonly IMachineProfileProcedureWebService profileProcedureService;

        private Bay bay;

        private MAS.AutomationService.Contracts.BayNumber bayNumber;

        private bool isExecutingProcedure;

        private bool isWaitingForResponse;

        private SubscriptionToken sensorsToken;

        private decimal systemError;

        #endregion

        #region Constructors

        public BaseProfileHeightCheckViewModel(
            IEventAggregator eventAggregator,
            IMachineProfileProcedureWebService profileProcedureService,
            IMachineModeService machineModeService,
            IBayManager bayManager)
            : base(PresentationMode.Installer)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.profileProcedureService = profileProcedureService ?? throw new ArgumentNullException(nameof(profileProcedureService));
            this.machineModeService = machineModeService ?? throw new ArgumentNullException(nameof(machineModeService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));

            this.InitializeNavigationMenu();
        }

        #endregion

        #region Properties

        public Bay Bay => this.bay;

        public MAS.AutomationService.Contracts.BayNumber BayNumber
        {
            get => this.bayNumber;
            private set => this.SetProperty(ref this.bayNumber, value);
        }

        public virtual string Error => string.Empty;

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            protected set
            {
                if (this.SetProperty(ref this.isExecutingProcedure, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
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
                        this.ShowNotification(string.Empty);
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

        public decimal SystemError { get => this.systemError; set => this.SetProperty(ref this.systemError, value); }

        public string Title
        {
            get
            {
                string title = InstallationApp.Gate1HeightControl;

                if (this.BayNumber == MAS.AutomationService.Contracts.BayNumber.BayTwo)
                {
                    title = InstallationApp.Gate2HeightControl;
                }
                else if (this.BayNumber == MAS.AutomationService.Contracts.BayNumber.BayThree)
                {
                    title = InstallationApp.Gate3HeightControl;
                }

                return $"{title}";
            }
        }

        protected IBayManager BayManager => this.bayManager;

        protected IMachineProfileProcedureWebService ProfileProcedureService => this.profileProcedureService;

        #endregion

        #region Indexers

        public virtual string this[string columnName]
        {
            get
            {
                return null;
            }
        }

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

            this.ShowNotification(string.Empty);
        }

        public override async Task OnAppearedAsync()
        {
            this.ShowSteps();

            await base.OnAppearedAsync();

            this.sensorsToken = this.sensorsToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        this.OnSensorsChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data != null);

            await this.RetrieveCurrentPositionAsync();

            if (this.Data == null)
            {
                this.currentStep = ProfileHeightCheckStep.Initialize;
            }
            else
            {
                this.currentStep = (ProfileHeightCheckStep)Enum.Parse(typeof(ProfileHeightCheckStep), this.Data.ToString());
            }

            this.CheckMachinePowerAndMode();

            this.RaisePropertyChanged(nameof(this.Title));
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);

            this.ShowPrevStep(false, false);
            this.ShowNextStep(false, false);
            this.ShowAbortStep(false, false);
        }

        protected override void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            base.OnMachineModeChanged(e);

            if (e.MachinePower == Services.Models.MachinePowerState.Unpowered)
            {
                this.RestoreStates();
            }

            this.CheckMachinePowerAndMode();
        }

        protected virtual void RaiseCanExecuteChanged()
        {
        }

        protected void RestoreStates()
        {
            this.IsExecutingProcedure = false;

            this.RaiseCanExecuteChanged();
        }

        protected virtual void ShowSteps()
        {
        }

        protected void UpdateError()
        {
            this.IsExecutingProcedure = false;
        }

        private void CheckMachinePowerAndMode()
        {
            if (!this.IsEnabled)
            {
                this.ShowNotification(InstallationApp.MachineNotRunWarning, NotificationSeverity.Warning);
            }
            else if (this.machineModeService.MachineMode == MachineMode.Automatic)
            {
                this.ShowNotification(InstallationApp.MachineNotManualModeWarning, NotificationSeverity.Warning);
            }
            else
            {
                this.ShowNotification(string.Empty);
            }
        }

        private void InitializeNavigationMenu()
        {
            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.ProfileHeightCheck.STEP1,
                    nameof(Utils.Modules.Installation),
                    InstallationApp.Drawer,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.ProfileHeightCheck.STEP2,
                    nameof(Utils.Modules.Installation),
                    InstallationApp.Elevator,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.ProfileHeightCheck.STEP3,
                    nameof(Utils.Modules.Installation),
                    InstallationApp.Drawer,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.ProfileHeightCheck.STEP4,
                    nameof(Utils.Modules.Installation),
                    InstallationApp.Shape,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.ProfileHeightCheck.STEP5,
                    nameof(Utils.Modules.Installation),
                    InstallationApp.Calibration,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.ProfileHeightCheck.STEP6,
                    nameof(Utils.Modules.Installation),
                    InstallationApp.Result,
                    trackCurrentView: false));
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.RaiseCanExecuteChanged();
        }

        private async Task RetrieveCurrentPositionAsync()
        {
            try
            {
                this.bay = await this.bayManager.GetBayAsync();
                this.BayNumber = this.bay.Number;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                this.IsWaitingForResponse = false;
                this.IsExecutingProcedure = false;

                var currentBay = (int)this.BayNumber;
                await this.profileProcedureService.SaveAsync(currentBay);
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
