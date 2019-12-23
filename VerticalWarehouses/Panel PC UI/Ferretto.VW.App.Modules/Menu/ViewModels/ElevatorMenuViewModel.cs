using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Menu.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class ElevatorMenuViewModel : BaseInstallationMenuViewModel
    {
        #region Fields

        private DelegateCommand beltBurnishingCommand;

        private DelegateCommand verticalOffsetCalibration;

        private DelegateCommand verticalOriginCalibration;

        private DelegateCommand verticalResolutionCalibration;

        private DelegateCommand weightAnalysisCommand;

        private DelegateCommand weightMeasurement;

        #endregion

        #region Constructors

        public ElevatorMenuViewModel()
            : base()
        {
        }

        #endregion

        #region Enums

        private enum Menu
        {
            BeltBurnishing,

            WeightAnalysis,

            WeightMeasurement,

            VerticalOffsetCalibration,

            VerticalResolutionCalibration,

            VerticalOriginCalibration,
        }

        #endregion

        #region Properties

        public ICommand BeltBurnishingCommand =>
            this.beltBurnishingCommand
            ??
            (this.beltBurnishingCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.BeltBurnishing),
                this.CanExecuteCommand));

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand VerticalOffsetCalibrationCommand =>
            this.verticalOffsetCalibration
            ??
            (this.verticalOffsetCalibration = new DelegateCommand(
                () => this.ExecuteCommand(Menu.VerticalOffsetCalibration),
                this.CanExecuteCommand));

        public ICommand VerticalOriginCalibrationCommand =>
            this.verticalOriginCalibration
            ??
            (this.verticalOriginCalibration = new DelegateCommand(
                () => this.ExecuteCommand(Menu.VerticalOriginCalibration),
                this.CanExecuteCommand));

        public ICommand VerticalResolutionCalibrationCommand =>
            this.verticalResolutionCalibration
            ??
            (this.verticalResolutionCalibration = new DelegateCommand(
                () => this.ExecuteCommand(Menu.VerticalResolutionCalibration),
                this.CanExecuteCommand));

        public ICommand WeightAnalysisCommand =>
            this.weightAnalysisCommand
            ??
            (this.weightAnalysisCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.WeightAnalysis),
                this.CanExecuteCommand));

        public ICommand WeightMeasurementCommand =>
            this.weightMeasurement
            ??
            (this.weightMeasurement = new DelegateCommand(
                () => this.ExecuteCommand(Menu.WeightMeasurement),
                this.CanExecuteCommand));

        #endregion

        #region Methods

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.beltBurnishingCommand?.RaiseCanExecuteChanged();
            this.verticalOffsetCalibration?.RaiseCanExecuteChanged();
            this.verticalOriginCalibration?.RaiseCanExecuteChanged();
            this.verticalResolutionCalibration?.RaiseCanExecuteChanged();
            this.weightAnalysisCommand?.RaiseCanExecuteChanged();
            this.weightMeasurement?.RaiseCanExecuteChanged();
        }

        private void ExecuteCommand(Menu menu)
        {
            switch (menu)
            {
                case Menu.BeltBurnishing:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.BELTBURNISHING,
                       data: null,
                       trackCurrentView: true);
                    break;

                case Menu.WeightAnalysis:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.WEIGHTANALYSIS,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.WeightMeasurement:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.Elevator.WeightCheck.STEP1,
                       data: null,
                       trackCurrentView: true);
                    break;

                case Menu.VerticalOffsetCalibration:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.VerticalOffsetCalibration.STEP1,
                       data: null,
                       trackCurrentView: true);
                    break;

                case Menu.VerticalResolutionCalibration:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.VerticalResolutionCalibration.STEP1,
                       data: null,
                       trackCurrentView: true);
                    break;

                case Menu.VerticalOriginCalibration:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.VERTICALORIGINCALIBRATION,
                        data: null,
                        trackCurrentView: true);
                    break;
            }
        }

        #endregion
    }
}
