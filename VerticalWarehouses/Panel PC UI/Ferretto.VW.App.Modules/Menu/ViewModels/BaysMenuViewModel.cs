﻿using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Menu.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class BaysMenuViewModel : BaseInstallationMenuViewModel
    {
        #region Fields

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly IMachineSetupStatusWebService machineSetupStatusWebService;

        private DelegateCommand bayControlCommand;

        private DelegateCommand bayHeightCommand;

        private DelegateCommand carouselCalibrationCommand;

        private DelegateCommand doubleExternalBayTestCommand;

        private DelegateCommand externalBayCalibrationCommand;

        private bool isSpeaActive;

        private DelegateCommand sensitiveSensorCommand;

        private DelegateCommand testShutterCommand;

        #endregion

        #region Constructors

        public BaysMenuViewModel(
            IMachineSetupStatusWebService machineSetupStatusWebService,
            IMachineIdentityWebService machineIdentityWebService)
            : base()
        {
            this.machineSetupStatusWebService = machineSetupStatusWebService ?? throw new ArgumentNullException(nameof(machineSetupStatusWebService));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));

            this.SetupStatusCapabilities = new SetupStatusCapabilities();
        }

        #endregion

        #region Enums

        private enum Menu
        {
            BayControl,

            BayHeight,

            CarouselCalibration,

            ExternalBayCalibration,

            TestShutter,

            BEDTest,

            SensitiveSensor
        }

        #endregion

        #region Properties

        public ICommand BayControlCommand =>
            this.bayControlCommand
            ??
            (this.bayControlCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.BayControl),
                () => this.CanExecuteCommand() &&
                      (this.BayControl.CanBePerformed || ConfigurationManager.AppSettings.GetOverrideSetupStatus())));

        public ICommand BayHeightCommand =>
            this.bayHeightCommand
            ??
            (this.bayHeightCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.BayHeight),
                () => this.CanExecuteCommand() &&
                      (true || ConfigurationManager.AppSettings.GetOverrideSetupStatus())));

        public ICommand CarouselCalibrationCommand =>
                    this.carouselCalibrationCommand
            ??
            (this.carouselCalibrationCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.CarouselCalibration),
                () => this.CanExecuteCommand() &&
               (true || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
                ));

        public ICommand DoubleExternalBayTestCommand =>
                    this.doubleExternalBayTestCommand
            ??
            (this.doubleExternalBayTestCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.BEDTest),
                () => this.CanExecuteCommand() &&
               (true || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
                ));

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand ExternalBayCalibrationCommand =>
                    this.externalBayCalibrationCommand
            ??
            (this.externalBayCalibrationCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.ExternalBayCalibration),
                () => this.CanExecuteCommand() &&
               (true || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
                ));

        public bool IsBayControlBypassed => this.BayControl.IsBypassed;

        public bool IsBayControlCompleted => this.BayControl.IsCompleted && !this.BayControl.IsBypassed;

        public bool IsBayProfileBypassed => this.BayProfile.IsBypassed;

        public bool IsBayProfileCompleted => this.BayProfile.IsCompleted && !this.BayProfile.IsBypassed;

        public bool IsBEDTestVisible => this.MachineService.Bay.IsExternal && this.MachineService.Bay.IsDouble;

        public bool IsCarouselCalibrationBypassed => this.CarouselCalibration.IsBypassed;

        public bool IsCarouselCalibrationCompleted => this.CarouselCalibration.IsCompleted && !this.CarouselCalibration.IsBypassed;

        public bool IsCarouselCalibrationVisible => this.MachineService.HasCarousel;

        public bool IsExternalBayCalibrationBypassed => this.ExternalBayCalibration.IsBypassed;

        public bool IsExternalBayCalibrationCompleted => this.ExternalBayCalibration.IsCompleted && !this.ExternalBayCalibration.IsBypassed;

        public bool IsExternalBayCalibrationVisible => this.MachineService.Bays.Any(f => f.IsExternal && !f.IsDouble);

        public bool IsSpeaActive
        {
            get => this.isSpeaActive;
            set => this.SetProperty(ref this.isSpeaActive, value);
        }

        public bool IsTestShutterBypassed => this.BayShutter.IsBypassed;

        // TODO - decide how to perform an external bay calibration test
        public bool IsTestShutterCompleted => this.BayShutter.IsCompleted && !this.BayShutter.IsBypassed;

        public bool IsTestShutterVisible => this.MachineService.HasShutter;

        public ICommand SensitiveSensorCommand =>
                                                                                                                                                                            this.sensitiveSensorCommand
                    ??
                    (this.sensitiveSensorCommand = new DelegateCommand(
                        () => this.ExecuteCommand(Menu.SensitiveSensor),
                        () => (true || ConfigurationManager.AppSettings.GetOverrideSetupStatus())));

        public SetupStatusCapabilities SetupStatusCapabilities { get; private set; }

        public ICommand TestShutterCommand =>
                    this.testShutterCommand
            ??
            (this.testShutterCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.TestShutter),
                () => this.CanExecuteCommand() &&
                      (this.BayShutter.CanBePerformed || ConfigurationManager.AppSettings.GetOverrideSetupStatus())));

        private SetupStepStatus BayControl => this.BaySetupStatus?.Check ?? new SetupStepStatus();

        private SetupStepStatus BayProfile => this.BaySetupStatus?.Profile ?? new SetupStepStatus();

        private BaySetupStatus BaySetupStatus
        {
            get
            {
                BaySetupStatus res = null;

                switch (this.MachineService.BayNumber)
                {
                    case BayNumber.BayOne:
                        res = this.SetupStatusCapabilities?.Bay1;
                        break;

                    case BayNumber.BayTwo:
                        res = this.SetupStatusCapabilities?.Bay2;
                        break;

                    case BayNumber.BayThree:
                        res = this.SetupStatusCapabilities?.Bay3;
                        break;
                }

                return res;
            }
        }

        private SetupStepStatus BayShutter => this.BaySetupStatus?.Shutter ?? new SetupStepStatus();

        private SetupStepStatus CarouselCalibration => this.BaySetupStatus?.CarouselCalibration ?? new SetupStepStatus();

        private SetupStepStatus ExternalBayCalibration => this.BaySetupStatus?.ExternalBayCalibration ?? new SetupStepStatus();

        private SetupStepStatus VerticalOriginCalibration => this.SetupStatusCapabilities?.VerticalOriginCalibration ?? new SetupStepStatus();

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.RaisePropertyChanged(nameof(this.IsBayControlBypassed));
            this.RaisePropertyChanged(nameof(this.IsBayControlCompleted));
            this.RaisePropertyChanged(nameof(this.IsBayProfileBypassed));
            this.RaisePropertyChanged(nameof(this.IsBayProfileCompleted));
            this.RaisePropertyChanged(nameof(this.IsTestShutterBypassed));
            this.RaisePropertyChanged(nameof(this.IsTestShutterCompleted));
            this.RaisePropertyChanged(nameof(this.IsCarouselCalibrationBypassed));
            this.RaisePropertyChanged(nameof(this.IsCarouselCalibrationCompleted));

            this.IsSpeaActive = await this.machineIdentityWebService.GetIsSpeaEnableAsync();

            //this.RaisePropertyChanged(nameof(this.IsCarouselCalibrationVisible));
            //this.RaisePropertyChanged(nameof(this.IsExternalBayCalibrationVisible));

            this.RaisePropertyChanged(nameof(this.IsExternalBayCalibrationBypassed));
            this.RaisePropertyChanged(nameof(this.IsExternalBayCalibrationCompleted));

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            this.SetupStatusCapabilities = this.MachineService.SetupStatus;

            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsBayControlCompleted));
            this.RaisePropertyChanged(nameof(this.IsBayProfileCompleted));
            this.RaisePropertyChanged(nameof(this.IsTestShutterCompleted));

            this.bayControlCommand?.RaiseCanExecuteChanged();
            this.bayHeightCommand?.RaiseCanExecuteChanged();
            this.testShutterCommand?.RaiseCanExecuteChanged();
            this.carouselCalibrationCommand?.RaiseCanExecuteChanged();
            this.externalBayCalibrationCommand?.RaiseCanExecuteChanged();
            this.doubleExternalBayTestCommand?.RaiseCanExecuteChanged();
        }

        private void ExecuteCommand(Menu menu)
        {
            switch (menu)
            {
                case Menu.BayControl:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.Bays.BAYCHECK,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.BayHeight:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.PROFILERESOLUTIONVIEW,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.CarouselCalibration:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.CAROUSELCALIBRATION,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.ExternalBayCalibration:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.EXTERNALBAYCALIBRATION,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.TestShutter:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.SHUTTERENDURANCETEST,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.BEDTest:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.BEDTEST,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.SensitiveSensor:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.SENSITIVEALAMR,
                        data: null,
                        trackCurrentView: true);
                    break;
            }
        }

        #endregion
    }
}
