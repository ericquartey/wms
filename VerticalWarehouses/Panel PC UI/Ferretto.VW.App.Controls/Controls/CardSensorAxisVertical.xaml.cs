using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for CardSensorAxisVertical
    /// </summary>
    public partial class CardSensorAxisVertical : UserControl
    {
        #region Fields

        [Browsable(false)]
        public static readonly DependencyProperty ElevatorVerticalPositionProperty =
            DependencyProperty.Register(nameof(ElevatorVerticalPosition), typeof(double?), typeof(CardSensorAxisVertical));

        [Browsable(false)]
        public static readonly DependencyProperty EmbarkedLoadingUnitProperty =
            DependencyProperty.Register(nameof(EmbarkedLoadingUnit), typeof(LoadingUnit), typeof(CardSensorAxisVertical));

        [Browsable(false)]
        public static readonly DependencyProperty SensorsServiceProperty =
            DependencyProperty.Register(nameof(SensorsService), typeof(ISensorsService), typeof(CardSensorAxisVertical));

        [Browsable(false)]
        public static readonly DependencyProperty VerticalDescriptionProperty =
            DependencyProperty.Register(nameof(VerticalDescription), typeof(string), typeof(CardSensorAxisVertical));

        private IEventAggregator eventAggregator;

        private IMachineService machineService;

        private SubscriptionToken machineStatusChangesToken;

        private ISensorsService sensorsService;

        private IThemeService themeService;

        #endregion

        #region Constructors

        public CardSensorAxisVertical()
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

        public double? ElevatorVerticalPosition
        {
            get => (double?)this.GetValue(ElevatorVerticalPositionProperty);
            set => this.SetValue(ElevatorVerticalPositionProperty, value);
        }

        public LoadingUnit EmbarkedLoadingUnit
        {
            get => (LoadingUnit)this.GetValue(EmbarkedLoadingUnitProperty);
            set => this.SetValue(EmbarkedLoadingUnitProperty, value);
        }

        public ISensorsService SensorsService
        {
            get => (ISensorsService)this.GetValue(SensorsServiceProperty);
            set => this.SetValue(SensorsServiceProperty, value);
        }

        public string VerticalDescription
        {
            get => (string)this.GetValue(VerticalDescriptionProperty);
            set => this.SetValue(VerticalDescriptionProperty, value);
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
        }

        protected void OnDataRefresh()
        {
            this.SensorsService = this.sensorsService;
            this.EmbarkedLoadingUnit = this.machineService.MachineStatus.EmbarkedLoadingUnit;
            this.ElevatorVerticalPosition = this.machineService.MachineStatus.ElevatorVerticalPosition;
            this.VerticalDescription = string.Empty;
            if (!(this.machineService.MachineStatus.VerticalTargetPosition is null))
            {
                this.VerticalDescription += $"Target: {this.machineService.MachineStatus.VerticalTargetPosition?.ToString("F0")}";
            }

            if (!(this.machineService.MachineStatus.VerticalSpeed is null))
            {
                this.VerticalDescription += $" Speed: {this.machineService.MachineStatus.VerticalSpeed?.ToString("F0")}";
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
