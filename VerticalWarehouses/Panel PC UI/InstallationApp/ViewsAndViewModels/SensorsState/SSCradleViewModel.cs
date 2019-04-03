using Ferretto.VW.Common_Utils.IO;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.InstallationApp.Interfaces;
using Ferretto.VW.InstallationApp.Resources;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSCradleViewModel : BindableBase, ISSCradleViewModel
    {
        #region Fields

        private IEventAggregator eventAggregator;

        private IUnityContainer container;

        private IOSensorsStatus ioSensorsStatus;

        private SubscriptionToken updateCradleSensorsState;

        private bool zeroPawlSensor;

        private bool luPresentiInMachineSide;

        private bool luPresentInOperatorSide = true;

        #endregion

        #region Constructors

        public SSCradleViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.ioSensorsStatus = new IOSensorsStatus();
        }

        #endregion

        #region Properties

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
            this.updateCradleSensorsState = this.eventAggregator.GetEvent<MAS_Event>()
                .Subscribe(
                message => this.UpdateCradleSensorsState((message.Data as INotificationMessageSensorsChangedData).SensorsStates),
                ThreadOption.PublisherThread,
                false,
                message => message.NotificationType == NotificationType.SensorsChanged);
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<MAS_Event>().Unsubscribe(this.updateCradleSensorsState);
        }

        private void UpdateCradleSensorsState(bool[] message)
        {
            this.ioSensorsStatus.UpdateInputStates(message);

            this.ZeroPawlSensor = this.ioSensorsStatus.ZeroPawl;
            this.LuPresentiInMachineSide = this.ioSensorsStatus.LuPresentiInMachineSide;
            this.LuPresentInOperatorSide = this.ioSensorsStatus.LuPresentInOperatorSide;
        }

        #endregion
    }
}
