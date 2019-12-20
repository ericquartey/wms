using System;
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

        //Sensor1="{Binding IsZeroChain}"
        //Sensor2="{Binding LuPresentInMachineSide}"
        //Sensor3="{Binding LuPresentInOperatorSide}"

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineService machineService;

        private readonly ISensorsService sensorsService;

        private SubscriptionToken machineStatusChangesToken;

        #endregion

        #region Constructors

        public CardSensorAxisHorizontal()
        {
            this.InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.Dispatcher.ShutdownStarted += this.Dispatcher_ShutdownStarted;

            this.eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            this.machineService = ServiceLocator.Current.GetInstance<IMachineService>();
            this.sensorsService = ServiceLocator.Current.GetInstance<ISensorsService>();

            this.Loaded += (s, e) =>
            {
                this.sensorsService.RefreshAsync(true);
                this.SensorsService = this.sensorsService;
                this.ElevatorHorizontalPosition = this.machineService.MachineStatus.ElevatorHorizontalPosition;
                this.HorizontalTargetPosition = this.machineService.MachineStatus.HorizontalTargetPosition;
            };

            this.machineStatusChangesToken = this.machineStatusChangesToken
                ?? this.eventAggregator
                    .GetEvent<MachineStatusChangedPubSubEvent>()
                    .Subscribe(
                        async (m) => await this.OnMachineStatusChangedAsync(m),
                        ThreadOption.UIThread,
                        false);

            this.DataContext = this;
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

        protected virtual Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            this.ElevatorHorizontalPosition = this.machineService.MachineStatus.ElevatorHorizontalPosition;
            this.HorizontalTargetPosition = this.machineService.MachineStatus.HorizontalTargetPosition;

            return Task.CompletedTask;
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            if (this.machineStatusChangesToken != null)
            {
                this.machineStatusChangesToken.Dispose();
                this.machineStatusChangesToken = null;
            }
        }

        #endregion
    }
}
