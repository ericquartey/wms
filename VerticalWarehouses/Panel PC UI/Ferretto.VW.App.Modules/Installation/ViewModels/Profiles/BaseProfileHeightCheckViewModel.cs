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
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

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

    public class BaseProfileHeightCheckViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineModeService machineModeService;

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private readonly IMachineProfileProcedureService profileProcedureService;

        private int bayNumber;

        private ProfileHeightCheckStep currentStep;

        private bool isExecutingProcedure;

        private bool isWaitingForResponse;

        private DelegateCommand saveCommand;

        private DelegateCommand step1Command;

        private DelegateCommand step2Command;

        private DelegateCommand step3Command;

        private DelegateCommand step4Command;

        private DelegateCommand step5Command;

        private DelegateCommand step6Command;

        private decimal systemError;

        #endregion

        #region Constructors

        public BaseProfileHeightCheckViewModel(
            IEventAggregator eventAggregator,
            IMachineProfileProcedureService profileProcedureService,
            IMachineModeService machineModeService,
            IBayManager bayManager)
            : base(PresentationMode.Installer)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (profileProcedureService is null)
            {
                throw new ArgumentNullException(nameof(profileProcedureService));
            }

            if (bayManager is null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            if (machineModeService is null)
            {
                throw new ArgumentNullException(nameof(machineModeService));
            }

            this.eventAggregator = eventAggregator;
            this.profileProcedureService = profileProcedureService;
            this.bayManager = bayManager;
            this.machineModeService = machineModeService;
            this.BayNumber = bayManager.Bay.Number;

            this.InitializeNavigationMenu();
        }

        #endregion

        #region Properties

        public int BayNumber
        {
            get => this.bayNumber;
            private set => this.SetProperty(ref this.bayNumber, value);
        }

        public virtual string Error => string.Join(Environment.NewLine, Environment.NewLine);

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

        public ICommand SaveCommand =>
                    this.saveCommand
            ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.SaveAsync()));

        public ICommand Step1Command =>
            this.step1Command
            ??
            (this.step1Command = new DelegateCommand(
                () => this.NavigateToStep1(),
                this.CanExecuteStepCommand));

        public ICommand Step2Command =>
            this.step2Command
            ??
            (this.step2Command = new DelegateCommand(
                () => this.NavigateToStep2(),
                this.CanExecuteStepCommand));

        public ICommand Step3Command =>
            this.step3Command
            ??
            (this.step3Command = new DelegateCommand(
                () => this.NavigateToStep3(),
                this.CanExecuteStep3Command));

        public ICommand Step4Command =>
            this.step4Command
            ??
            (this.step4Command = new DelegateCommand(
                () => this.NavigateToStep4(),
                this.CanExecuteStepCommand));

        public ICommand Step5Command =>
            this.step5Command
            ??
            (this.step5Command = new DelegateCommand(
                () => this.NavigateToStep5(),
                this.CanExecuteStepCommand));

        public ICommand Step6Command =>
            this.step6Command
            ??
            (this.step6Command = new DelegateCommand(
                () => this.NavigateToStep6(),
                this.CanExecuteStepCommand));

        public decimal SystemError { get => this.systemError; set => this.SetProperty(ref this.systemError, value); }

        public string Title
        {
            get
            {
                string title = InstallationApp.Gate1HeightControl;
                if (this.bayManager.Bay.Number == 2)
                {
                    title = InstallationApp.Gate2HeightControl;
                }
                else if (this.bayManager.Bay.Number == 3)
                {
                    title = InstallationApp.Gate3HeightControl;
                }

                return $"{title}";
            }
        }

        protected IBayManager BayManager => this.bayManager;

        protected IMachineProfileProcedureService ProfileProcedureService => this.profileProcedureService;

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

            this.ShowNotification(string.Empty);
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.IsBackNavigationAllowed = true;

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

        protected virtual bool CanExecuteStep3Command()
        {
            return this.CanExecuteStepCommand();
        }

        protected virtual bool CanExecuteStepCommand()
        {
            return !this.IsExecutingProcedure
                && !this.IsWaitingForResponse
                && string.IsNullOrWhiteSpace(this.Error);
        }

        protected virtual void NavigateToStep1()
        {
            this.currentStep = ProfileHeightCheckStep.Initialize;

            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.ProfileHeightCheck.STEP1,
                data: this.currentStep,
                trackCurrentView: false);
        }

        protected virtual void NavigateToStep2()
        {
            this.currentStep = ProfileHeightCheckStep.ElevatorPosition;

            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.ProfileHeightCheck.STEP2,
                data: this.currentStep,
                trackCurrentView: false);
        }

        protected virtual void NavigateToStep3()
        {
            this.currentStep = ProfileHeightCheckStep.DrawerPosition;

            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.ProfileHeightCheck.STEP3,
                data: this.currentStep,
                trackCurrentView: false);
        }

        protected virtual void NavigateToStep4()
        {
            this.currentStep = ProfileHeightCheckStep.ShapePosition;

            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.ProfileHeightCheck.STEP4,
                data: this.currentStep,
                trackCurrentView: false);
        }

        protected virtual void NavigateToStep5()
        {
            this.currentStep = ProfileHeightCheckStep.TaraturaCatena;

            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.ProfileHeightCheck.STEP5,
                data: this.currentStep,
                trackCurrentView: false);
        }

        protected virtual void NavigateToStep6()
        {
            this.currentStep = ProfileHeightCheckStep.ResultCheck;

            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.ProfileHeightCheck.STEP6,
                data: this.currentStep,
                trackCurrentView: false);
        }

        protected override void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            base.OnMachineModeChanged(e);
            this.CheckMachinePowerAndMode();
        }

        protected virtual void RaiseCanExecuteChanged()
        {
            this.step1Command?.RaiseCanExecuteChanged();
            this.step2Command?.RaiseCanExecuteChanged();
            this.step3Command?.RaiseCanExecuteChanged();
            this.step4Command?.RaiseCanExecuteChanged();
            this.step5Command?.RaiseCanExecuteChanged();
            this.step6Command?.RaiseCanExecuteChanged();
            this.saveCommand?.RaiseCanExecuteChanged();
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
                    InstallationApp.Shutter,
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

        private async Task SaveAsync()
        {
            try
            {
                this.IsWaitingForResponse = false;
                this.IsExecutingProcedure = false;

                var currentBay = this.bayManager.Bay.Number;
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
