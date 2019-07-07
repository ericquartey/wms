using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.IO;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_AutomationService.Contracts;
using Ferretto.VW.MAS_Utils.Events;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.InstallationApp
{
    public class SSCradleViewModel : BindableBase, ISSCradleViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IOSensorsStatus ioSensorsStatus;

        private IUnityContainer container;

        private bool luPresentiInMachineSide;

        private bool luPresentInOperatorSide;

        private SubscriptionToken updateCradleSensorsState;

        private IUpdateSensorsService updateSensorsService;

        private bool zeroPawlSensor;

        #endregion

        #region Constructors

        public SSCradleViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.ioSensorsStatus = new IOSensorsStatus();
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public bool LuPresentiInMachineSide { get => this.luPresentiInMachineSide; set => this.SetProperty(ref this.luPresentiInMachineSide, value); }

        public bool LuPresentInOperatorSide { get => this.luPresentInOperatorSide; set => this.SetProperty(ref this.luPresentInOperatorSide, value); }

        public BindableBase NavigationViewModel { get; set; }

        public bool ZeroPawlSensor { get => this.zeroPawlSensor; set => this.SetProperty(ref this.zeroPawlSensor, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            this.UnSubscribeMethodFromEvent();
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.updateSensorsService = this.container.Resolve<IUpdateSensorsService>();
        }

        public async Task OnEnterViewAsync()
        {
            this.updateCradleSensorsState = this.eventAggregator.GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                message => this.UpdateCradleSensorsState(message.Data.SensorsStates),
                ThreadOption.PublisherThread,
                false);

            await this.updateSensorsService.ExecuteAsync();
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<NotificationEventUI<SensorsChangedMessageData>>().Unsubscribe(this.updateCradleSensorsState);
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
