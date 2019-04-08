 using Ferretto.VW.Common_Utils.IO;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.InstallationApp.Interfaces;
using Ferretto.VW.InstallationApp.Resources;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSVerticalAxisViewModel : BindableBase, ISSVerticalAxisViewModel
    {
        #region Fields

        private IEventAggregator eventAggregator;

        private IUnityContainer container;

        private IOSensorsStatus ioSensorsStatus;

        private SubscriptionToken updateVerticalandCradleSensorsState;

        private bool emergencyEndRun;

        private bool zeroVerticalSensor;

        private bool elevatorEngineSelected;

        private bool cradleEngineSelected;

        private bool zeroPawlSensor;

        private bool luPresentiInMachineSide;

        private bool luPresentInOperatorSide;

        #endregion

        #region Constructors

        public SSVerticalAxisViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.ioSensorsStatus = new IOSensorsStatus();
        }

        #endregion

        #region Properties

        public bool EmergencyEndRun { get => this.emergencyEndRun; set => this.SetProperty(ref this.emergencyEndRun, value); }

        public bool ZeroVerticalSensor { get => this.zeroVerticalSensor; set => this.SetProperty(ref this.zeroVerticalSensor, value); }

        public bool ElevatorEngineSelected { get => this.elevatorEngineSelected; set => this.SetProperty(ref this.elevatorEngineSelected, value); }

        public bool CradleEngineSelected { get => this.cradleEngineSelected; set => this.SetProperty(ref this.cradleEngineSelected, value); }

        public bool ZeroPawlSensor { get => this.zeroPawlSensor; set => this.SetProperty(ref this.zeroPawlSensor, value); }

        public bool LuPresentiInMachineSide { get => this.luPresentiInMachineSide; set => this.SetProperty(ref this.luPresentiInMachineSide, value); }

        public bool LuPresentInOperatorSide { get => this.luPresentInOperatorSide; set => this.SetProperty(ref this.luPresentInOperatorSide, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            this.UnSubscribeMethodFromEvent();
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
        }

        public void SubscribeMethodToEvent()
        {
            this.updateVerticalandCradleSensorsState = this.eventAggregator.GetEvent<MAS_Event>()
                .Subscribe(
                message => this.UpdateVerticalandCradleSensorsState((message.Data as INotificationMessageSensorsChangedData).SensorsStates),
                ThreadOption.PublisherThread,
                false,
                message => message.NotificationType == NotificationType.SensorsChanged);
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<MAS_Event>().Unsubscribe(this.updateVerticalandCradleSensorsState);
        }

        private void UpdateVerticalandCradleSensorsState(bool[] message)
        {
            this.ioSensorsStatus.UpdateInputStates(message);

            this.EmergencyEndRun = this.ioSensorsStatus.EmergencyEndRun;
            this.ZeroVerticalSensor = this.ioSensorsStatus.ZeroVertical;
            this.ElevatorEngineSelected = this.ioSensorsStatus.ElevatorMotorSelected;
            this.CradleEngineSelected = this.ioSensorsStatus.CradleMotorSelected;
            this.ZeroPawlSensor = this.ioSensorsStatus.ZeroPawl;
            this.LuPresentiInMachineSide = this.ioSensorsStatus.LuPresentiInMachineSide;
            this.LuPresentInOperatorSide = this.ioSensorsStatus.LuPresentInOperatorSide;
        }

        #endregion
    }
}
