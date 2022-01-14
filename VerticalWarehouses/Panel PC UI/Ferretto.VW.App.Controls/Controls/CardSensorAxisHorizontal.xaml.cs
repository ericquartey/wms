using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommonServiceLocator;
using Ferretto.VW.App.Services;
using Prism.Events;
using Ferretto.VW.App.Controls.Skins;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for CardSensorAxisHorizontal
    /// </summary>
    public partial class CardSensorAxisHorizontal : UserControl
    {
        #region Fields

        [Browsable(false)]
        public static readonly DependencyProperty ElevatorHorizontalPositionProperty =
            DependencyProperty.Register(nameof(ElevatorHorizontalPosition), typeof(double?), typeof(CardSensorAxisHorizontal));

        [Browsable(false)]
        public static readonly DependencyProperty HorizontalTargetPositionProperty =
            DependencyProperty.Register(nameof(HorizontalTargetPosition), typeof(double?), typeof(CardSensorAxisHorizontal));

        [Browsable(false)]
        public static readonly DependencyProperty SensorsServiceProperty =
            DependencyProperty.Register(nameof(SensorsService), typeof(ISensorsService), typeof(CardSensorAxisHorizontal));

        private IEventAggregator eventAggregator;

        private IMachineService machineService;

        private SubscriptionToken machineStatusChangesToken;

        private ISensorsService sensorsService;

        private IThemeService themeService;

        #endregion

        #region Constructors

        public CardSensorAxisHorizontal()
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

        public double? ElevatorHorizontalPosition
        {
            get => (double?)this.GetValue(ElevatorHorizontalPositionProperty);
            set => this.SetValue(ElevatorHorizontalPositionProperty, value);
        }

        public double? HorizontalTargetPosition
        {
            get => (double?)this.GetValue(HorizontalTargetPositionProperty);
            set => this.SetValue(HorizontalTargetPositionProperty, value);
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

        protected void OnAppeared()
        {
            this.SubscribeToEvents();

            this.OnDataRefresh();

            var converter = new BrushConverter();

            if (this.themeService.ActiveTheme == App.Services.Models.ApplicationTheme.Dark)
            {
                this.CardSensor.Background = (Brush)converter.ConvertFromString("#3C3C3C");
            }
            else
            {
                this.CardSensor.Background = (Brush)converter.ConvertFromString("#CCCCCC");
            }
        }

        protected void OnDataRefresh()
        {
            this.SensorsService = this.sensorsService;
            this.ElevatorHorizontalPosition = this.machineService.MachineStatus.ElevatorHorizontalPosition;
            this.HorizontalTargetPosition = this.machineService.MachineStatus.HorizontalTargetPosition;

            var zero = this.SensorsService.IsZeroChain;
            var ant = this.SensorsService.Sensors.LuPresentInOperatorSide;
            var post = this.SensorsService.Sensors.LuPresentInMachineSide;

            var converter = new BrushConverter();

            Brush FerrettoRed = (Brush)converter.ConvertFromString("#DA0010");

            Brush PpcBackground;

            if (this.themeService.ActiveTheme == App.Services.Models.ApplicationTheme.Dark)
            {
                PpcBackground = (Brush)converter.ConvertFromString("#3C3C3C");
            }
            else
            {
                PpcBackground = (Brush)converter.ConvertFromString("#CCCCCC");
            }

            if (this.SensorsService.IsBypass
                && ((!zero && !ant && !post)
                || ant != post))
            {
                this.CardSensor.Background = FerrettoRed;
            }
            else
            {
                this.CardSensor.Background = PpcBackground;
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
