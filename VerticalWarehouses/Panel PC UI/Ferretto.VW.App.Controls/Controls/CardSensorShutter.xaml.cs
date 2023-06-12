using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Ferretto.VW.App.Services;
using Prism.Events;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for CardSensorShutter
    /// </summary>
    public partial class CardSensorShutter : UserControl
    {
        #region Fields

        [Browsable(false)]
        public static readonly DependencyProperty SensorsServiceProperty =
            DependencyProperty.Register(nameof(SensorsService), typeof(ISensorsService), typeof(CardSensorShutter));

        [Browsable(false)]
        public static readonly DependencyProperty SensorAProperty =
            DependencyProperty.Register(nameof(SensorA), typeof(bool), typeof(CardSensorShutter));

        [Browsable(false)]
        public static readonly DependencyProperty SensorBProperty =
            DependencyProperty.Register(nameof(SensorB), typeof(bool), typeof(CardSensorShutter));

        [Browsable(false)]
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(nameof(Status), typeof(string), typeof(CardSensorShutter));

        public IMachineService machineService;

        private IEventAggregator eventAggregator;

        private SubscriptionToken machineStatusChangesToken;

        private ISensorsService sensorsService;

        private IThemeService themeService;

        #endregion

        #region Constructors

        public CardSensorShutter()
        {
            this.InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.DataContext = this;

            this.Loaded += (s, e) =>
            {
                this.OnAppeared();
            };
            this.Unloaded += (s, e) =>
            {
                this.Disappear();
            };
        }

        #endregion

        #region Properties

        public ISensorsService SensorsService
        {
            get => (ISensorsService)this.GetValue(SensorsServiceProperty);
            set => this.SetValue(SensorsServiceProperty, value);
        }

        public bool SensorA
        {
            get => (bool)this.GetValue(SensorAProperty);
            set => this.SetValue(SensorAProperty, value);
        }

        public bool SensorB
        {
            get => (bool)this.GetValue(SensorBProperty);
            set => this.SetValue(SensorBProperty, value);
        }

        public string Status
        {
            get => (string)this.GetValue(StatusProperty);
            set => this.SetValue(StatusProperty, value);
        }

        #endregion

        #region Methods

        protected void Disappear()
        {
            if (this.machineStatusChangesToken != null)
            {
                this.eventAggregator
                    .GetEvent<PubSubEvent<NavigationCompletedEventArgs>>()
                    .Unsubscribe(this.machineStatusChangesToken);

                this.machineStatusChangesToken.Dispose();
                this.machineStatusChangesToken = null;
            }

            this.sensorsService = null;
            this.eventAggregator = null;
            this.machineService = null;
        }

        protected void OnAppeared()
        {
            this.SubscribeToEvents();

            this.OnDataRefresh();

            this.Visibility = this.machineService.HasShutter ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        protected void OnDataRefresh()
        {
            this.SensorsService = this.sensorsService;






            switch (this.machineService.BayNumber)
            {
                case MAS.AutomationService.Contracts.BayNumber.BayOne:
                    this.Status = this.sensorsService.ShutterSensorsBay1.Status;
                    this.SensorA = this.SensorsService.Sensors.AGLSensorAShutterBay1;
                    this.SensorB = this.SensorsService.Sensors.AGLSensorBShutterBay1;
                    break;

                case MAS.AutomationService.Contracts.BayNumber.BayTwo:
                    this.Status = this.sensorsService.ShutterSensorsBay2.Status;
                    this.SensorA = this.SensorsService.Sensors.AGLSensorAShutterBay2;
                    this.SensorB = this.SensorsService.Sensors.AGLSensorBShutterBay2;
                    break;

                case MAS.AutomationService.Contracts.BayNumber.BayThree:
                    this.Status = this.sensorsService.ShutterSensorsBay3.Status;
                    this.SensorA = this.SensorsService.Sensors.AGLSensorAShutterBay3;
                    this.SensorB = this.SensorsService.Sensors.AGLSensorBShutterBay3;
                    break;

                default:
                    break;
            }
        }

        protected Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            this.OnDataRefresh();
            return Task.CompletedTask;
        }

        private void SubscribeToEvents()
        {
            this.eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            this.machineService = ServiceLocator.Current.GetInstance<IMachineService>();
            this.sensorsService = ServiceLocator.Current.GetInstance<ISensorsService>();
            this.themeService = ServiceLocator.Current.GetInstance<IThemeService>();

            this.machineStatusChangesToken = this.machineStatusChangesToken
                ?? this.eventAggregator
                    .GetEvent<MachineStatusChangedPubSubEvent>()
                    .Subscribe(
                        async (m) => await this.OnMachineStatusChangedAsync(m),
                        ThreadOption.UIThread,
                        false);
        }

        #endregion
    }
}
