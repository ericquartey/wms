using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class SensitiveAlarmViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private DelegateCommand sensitiveCarpetCommand;

        private bool sensitiveCarpetSensor;

        private string sensitiveCarpetText;

        private DelegateCommand sensitiveEdgeCommand;

        private bool sensitiveEdgeSensor;

        private string sensitiveEdgeText;

        #endregion

        #region Constructors

        public SensitiveAlarmViewModel(
            IMachineSensorsWebService machineSensorsWebService,
            IMachineIdentityWebService machineIdentityWebService)
            : base(PresentationMode.Installer)
        {
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
        }

        #endregion

        #region Enums

        private enum SensType
        {
            SensitiveEdgeSensor,

            SensitiveCarpetSensor
        }

        #endregion

        #region Properties

        public ICommand SensitiveCarpetCommand =>
                            this.sensitiveCarpetCommand
            ??
            (this.sensitiveCarpetCommand = new DelegateCommand(async () => await this.ChangeSensorAsync(SensType.SensitiveCarpetSensor), () => true));

        public bool SensitiveCarpetSensor
        {
            get => this.sensitiveCarpetSensor;
            set
            {
                this.SetProperty(ref this.sensitiveCarpetSensor, value);

                this.SensitiveCarpetText = value ? InstallationApp.AccessoryEnabled : General.WmsDisabled;
            }
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public string SensitiveCarpetText
        {
            get => this.sensitiveCarpetText;
            set => this.SetProperty(ref this.sensitiveCarpetText, value);
        }

        public ICommand SensitiveEdgeCommand =>
                            this.sensitiveEdgeCommand
            ??
            (this.sensitiveEdgeCommand = new DelegateCommand(async () => await this.ChangeSensorAsync(SensType.SensitiveEdgeSensor), () => true));

        public bool SensitiveEdgeSensor
        {
            get => this.sensitiveEdgeSensor;
            set
            {
                this.SetProperty(ref this.sensitiveEdgeSensor, value);

                this.SensitiveEdgeText = value ? InstallationApp.AccessoryEnabled : General.WmsDisabled;
            }
        }

        public string SensitiveEdgeText
        {
            get => this.sensitiveEdgeText;
            set => this.SetProperty(ref this.sensitiveEdgeText, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            try
            {
                this.SensitiveEdgeSensor = await this.machineIdentityWebService.GetSensitiveEdgeAlarmEnableAsync();
                this.SensitiveCarpetSensor = await this.machineIdentityWebService.GetSensitiveCarpetsAlarmEnableAsync();
            }
            catch (Exception)
            {
            }

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.sensitiveEdgeCommand?.RaiseCanExecuteChanged();
            this.sensitiveCarpetCommand?.RaiseCanExecuteChanged();
        }

        private async Task ChangeSensorAsync(SensType type)
        {
            try
            {
                switch (type)
                {
                    case SensType.SensitiveEdgeSensor:
                        this.SensitiveEdgeSensor = !this.SensitiveEdgeSensor;
                        await this.machineIdentityWebService.SetSensitiveEdgeBypassAsync(this.SensitiveEdgeSensor);
                        break;

                    case SensType.SensitiveCarpetSensor:
                        this.SensitiveCarpetSensor = !this.SensitiveCarpetSensor;
                        await this.machineIdentityWebService.SetSensitiveCarpetsBypassAsync(this.SensitiveCarpetSensor);
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}
