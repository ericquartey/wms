using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
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
        public static readonly DependencyProperty MachineStatusProperty =
            DependencyProperty.Register(nameof(MachineStatus), typeof(MachineStatus), typeof(CardSensorBay));

        [Browsable(false)]
        public static readonly DependencyProperty SensorsServiceProperty =
            DependencyProperty.Register(nameof(SensorsService), typeof(ISensorsService), typeof(CardSensorBay));

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineService machineService;

        private readonly ISensorsService sensorsService;

        private SubscriptionToken machineStatusChangesToken;

        #endregion

        #region Constructors

        public CardSensorBay()
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
                this.MachineStatus = this.machineService.MachineStatus;
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

        public MachineStatus MachineStatus
        {
            get => (MachineStatus)this.GetValue(MachineStatusProperty);
            set => this.SetValue(MachineStatusProperty, value);
        }

        public ISensorsService SensorsService
        {
            get => (ISensorsService)this.GetValue(SensorsServiceProperty);
            set => this.SetValue(SensorsServiceProperty, value);
        }

        #endregion

        #region Methods

        protected Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            this.MachineStatus = e.MachineStatus;

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
