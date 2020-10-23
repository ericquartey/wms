using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Prism.Events;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for CardSensorAxisBay
    /// </summary>
    public partial class CardSensorBay : UserControl
    {
        #region Fields

        [Browsable(false)]
        public static readonly DependencyProperty BayNumberProperty =
            DependencyProperty.Register(nameof(BayNumber), typeof(string), typeof(CardSensorBay));

        [Browsable(false)]
        public static readonly DependencyProperty CardBayPositionProperty =
            DependencyProperty.Register(nameof(CardBayPosition), typeof(string), typeof(CardSensorBay), new PropertyMetadata(string.Empty));

        [Browsable(false)]
        public static readonly DependencyProperty CardSensorLabel1Property =
            DependencyProperty.Register(nameof(CardSensorLabel1), typeof(string), typeof(CardSensorBay));

        [Browsable(false)]
        public static readonly DependencyProperty CardSensorLabel2Property =
            DependencyProperty.Register(nameof(CardSensorLabel2), typeof(string), typeof(CardSensorBay));

        [Browsable(false)]
        public static readonly DependencyProperty CardSensorLabel3Property =
            DependencyProperty.Register(nameof(CardSensorLabel3), typeof(string), typeof(CardSensorBay));

        [Browsable(false)]
        public static readonly DependencyProperty MachineStatusProperty =
            DependencyProperty.Register(nameof(MachineStatus), typeof(App.Services.Models.MachineStatus), typeof(CardSensorBay));

        [Browsable(false)]
        public static readonly DependencyProperty SensorsServiceProperty =
            DependencyProperty.Register(nameof(SensorsService), typeof(ISensorsService), typeof(CardSensorBay));

        private IEventAggregator eventAggregator;

        private IMachineService machineService;

        private SubscriptionToken machineStatusChangesToken;

        private ISensorsService sensorsService;

        #endregion

        #region Constructors

        public CardSensorBay()
        {
            this.InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.DataContext = this;

            this.Loaded += (s, e) =>
            {
                this.OnAppearedAsync();
            };
            this.Unloaded += (s, e) =>
            {
                this.Disappear();
            };
        }

        #endregion

        #region Properties

        public string BayNumber
        {
            get => (string)this.GetValue(BayNumberProperty);
            set => this.SetValue(BayNumberProperty, value);
        }

        public string CardBayPosition
        {
            get => (string)this.GetValue(CardBayPositionProperty);
            set => this.SetValue(CardBayPositionProperty, value);
        }

        public string CardSensorLabel1
        {
            get => (string)this.GetValue(CardSensorLabel1Property);
            set => this.SetValue(CardSensorLabel1Property, value);
        }

        public string CardSensorLabel2
        {
            get => (string)this.GetValue(CardSensorLabel2Property);
            set => this.SetValue(CardSensorLabel2Property, value);
        }

        public string CardSensorLabel3
        {
            get => (string)this.GetValue(CardSensorLabel3Property);
            set => this.SetValue(CardSensorLabel3Property, value);
        }

        public App.Services.Models.MachineStatus MachineStatus
        {
            get => (App.Services.Models.MachineStatus)this.GetValue(MachineStatusProperty);
            set => this.SetValue(MachineStatusProperty, value);
        }

        public ISensorsService SensorsService
        {
            get => (ISensorsService)this.GetValue(SensorsServiceProperty);
            set => this.SetValue(SensorsServiceProperty, value);
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

            this.eventAggregator = null;
            this.sensorsService = null;
            this.machineService = null;
        }

        protected void OnAppearedAsync()
        {
            this.SubscribeToEvents();

            this.OnDataRefreshAsync();
        }

        protected void OnDataRefreshAsync()
        {
            this.SensorsService = this.sensorsService;
            this.MachineStatus = this.machineService.MachineStatus;

            this.BayNumber = $"{Localized.Get("OperatorApp.Bay")} {(int)this.machineService.Bay.Number}";

            if (this.machineService.Bay.IsDouble)
            {
                this.CardSensorLabel2 = Localized.Get("OperatorApp.Up");
                this.CardSensorLabel3 = Localized.Get("OperatorApp.Down");
            }
            else
            {
                if (this.machineService.BayFirstPositionIsUpper)
                {
                    this.CardSensorLabel2 = Localized.Get("OperatorApp.Bay");
                    this.CardSensorLabel3 = null;
                }
                else
                {
                    this.CardSensorLabel2 = null;
                    this.CardSensorLabel3 = Localized.Get("OperatorApp.Bay");
                }
                if (this.machineService.Bays.Any(f => f.IsExternal))
                {
                    this.CardSensorLabel2 = Localized.Get("InstallationApp.ExternalBayShort");
                    this.CardSensorLabel3 = Localized.Get("InstallationApp.InternalBayShort");
                }
            }

            if (this.machineService.HasBayWithInverter)
            {
                this.CardSensorLabel1 = Localized.Get("SensorCard.Zero");
            }
            else
            {
                this.CardSensorLabel1 = string.Empty;
            }
        }

        protected Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            this.OnDataRefreshAsync();
            return Task.CompletedTask;
        }

        private void SubscribeToEvents()
        {
            this.eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            this.machineService = ServiceLocator.Current.GetInstance<IMachineService>();
            this.sensorsService = ServiceLocator.Current.GetInstance<ISensorsService>();

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
