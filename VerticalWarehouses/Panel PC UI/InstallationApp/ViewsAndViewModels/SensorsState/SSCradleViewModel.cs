using System.Threading.Tasks;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SensorsState
{
    public class SSCradleViewModel : BindableBase, ISSCradleViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IUpdateSensorsMachineService updateSensorsService;

        private bool luPresentiInMachineSide;

        private bool luPresentInOperatorSide;

        private SubscriptionToken updateCradleSensorsState;

        private bool zeroPawlSensor;

        #endregion

        #region Constructors

        public SSCradleViewModel(
            IEventAggregator eventAggregator,
            IUpdateSensorsMachineService updateSensorsService)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (updateSensorsService == null)
            {
                throw new System.ArgumentNullException(nameof(updateSensorsService));
            }

            this.eventAggregator = eventAggregator;
            this.updateSensorsService = updateSensorsService;
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

        public async Task OnEnterViewAsync()
        {
            this.updateCradleSensorsState = this.eventAggregator
                .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                    message => this.UpdateCradleSensorsState(message.Data.SensorsStates),
                    ThreadOption.PublisherThread,
                    false);

            await this.updateSensorsService.ExecuteAsync();
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator
                .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Unsubscribe(this.updateCradleSensorsState);
        }

        private void UpdateCradleSensorsState(bool[] message)
        {
            //this.ioSensorsStatus.UpdateInputStates(message);

            //this.ZeroPawlSensor = this.ioSensorsStatus.ZeroPawl;
            //this.LuPresentiInMachineSide = this.ioSensorsStatus.LuPresentiInMachineSide;
            //this.LuPresentInOperatorSide = this.ioSensorsStatus.LuPresentInOperatorSide;
        }

        #endregion
    }
}
