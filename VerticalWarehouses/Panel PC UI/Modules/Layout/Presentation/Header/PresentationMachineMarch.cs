using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationMachineMarch : BasePresentation
    {
        #region Fields

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineSensorsService machineSensorsService;

        private readonly IMachineMachineStatusService machineStatusService;

        private bool isBusy;

        private bool isMachinePoweredOn;

        #endregion

        #region Constructors

        public PresentationMachineMarch(
            IMachineSensorsService machineSensorsService,
            IMachineMachineStatusService machineStatusService)
            : base(PresentationTypes.MachineMarch)
        {
            if (machineSensorsService == null)
            {
                throw new System.ArgumentNullException(nameof(machineSensorsService));
            }

            if (machineStatusService == null)
            {
                throw new System.ArgumentNullException(nameof(machineStatusService));
            }

            this.machineSensorsService = machineSensorsService;
            this.machineStatusService = machineStatusService;

            this.EventAggregator
                .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                    message => this.UpdateMachinePowerState(message?.Data.SensorsStates),
                    ThreadOption.PublisherThread,
                    false);

            this.machineSensorsService.ForceNotificationAsync();
        }

        #endregion

        #region Properties

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsMachinePoweredOn
        {
            get => this.isMachinePoweredOn;
            set => this.SetProperty(ref this.isMachinePoweredOn, value);
        }

        #endregion

        #region Methods

        public override async Task ExecuteAsync()
        {
            //this.IsBusy = true;

            try
            {
                if (this.IsMachinePoweredOn)
                {
                    await this.machineStatusService.PowerOffAsync();
                }
                else
                {
                    var messageBoxResult = System.Windows.MessageBox.Show("Confirmation operation?", "March", System.Windows.MessageBoxButton.YesNo);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        await this.machineStatusService.PowerOnAsync();
                    }
                    else
                    {
                        this.IsBusy = false;
                    }
                }
            }
            catch
            {
                // TODO: report error
                this.IsBusy = false;
            }
        }

        protected override bool CanExecute()
        {
            return !this.IsBusy;
        }

        private void UpdateMachinePowerState(bool[] sensorsStates)
        {
            if (sensorsStates == null)
            {
                this.logger.Warn("Unable to update machine power state: empty sensors state array received.");
                return;
            }

            var sensorIndex = (int)IOMachineSensors.NormalState;

            if (sensorsStates.Length > sensorIndex)
            {
                var isPoweredOn = sensorsStates[sensorIndex];

                if (this.IsBusy == true && this.IsMachinePoweredOn != isPoweredOn)
                {
                    this.IsBusy = false;
                }
                this.IsMachinePoweredOn = isPoweredOn;
            }
            else
            {
                this.logger.Warn("Unable to update machine power state: sensors state array length was shorter than expected.");
            }
        }

        #endregion
    }
}
