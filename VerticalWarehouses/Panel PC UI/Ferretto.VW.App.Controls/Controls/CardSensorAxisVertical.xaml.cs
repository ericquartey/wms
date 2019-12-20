using System;
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
        public static readonly DependencyProperty VerticalTargetPositionProperty =
            DependencyProperty.Register(nameof(VerticalTargetPosition), typeof(double?), typeof(CardSensorAxisVertical));

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineService machineService;

        private SubscriptionToken machineStatusChangesToken;

        #endregion

        #region Constructors

        public CardSensorAxisVertical()
        {
            this.InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.Loaded += (s, e) =>
            {
                this.EmbarkedLoadingUnit = this.machineService.MachineStatus.EmbarkedLoadingUnit;
                this.ElevatorVerticalPosition = this.machineService.MachineStatus.ElevatorVerticalPosition;
                this.VerticalTargetPosition = this.machineService.MachineStatus.VerticalTargetPosition;
            };

            this.Dispatcher.ShutdownStarted += this.Dispatcher_ShutdownStarted;

            this.eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            this.machineService = ServiceLocator.Current.GetInstance<IMachineService>();

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

        public double? VerticalTargetPosition
        {
            get => (double?)this.GetValue(VerticalTargetPositionProperty);
            set => this.SetValue(VerticalTargetPositionProperty, value);
        }

        #endregion

        #region Methods

        protected virtual Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            this.EmbarkedLoadingUnit = this.machineService.MachineStatus.EmbarkedLoadingUnit;
            this.ElevatorVerticalPosition = this.machineService.MachineStatus.ElevatorVerticalPosition;
            this.VerticalTargetPosition = this.machineService.MachineStatus.VerticalTargetPosition;

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
