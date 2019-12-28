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
    /// Interaction logic for CardSensorPosition
    /// </summary>
    public partial class CardSensorPosition : UserControl
    {
        #region Fields

        [Browsable(false)]
        public static readonly DependencyProperty MachineStatusProperty =
            DependencyProperty.Register(nameof(MachineStatus), typeof(MachineStatus), typeof(CardSensorPosition));

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineService machineService;

        private SubscriptionToken machineStatusChangesToken;

        #endregion

        #region Constructors

        public CardSensorPosition()
        {
            this.InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.Loaded += (s, e) =>
            {
                this.MachineStatus = this.machineService.MachineStatus;
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

        public MachineStatus MachineStatus
        {
            get => (MachineStatus)this.GetValue(MachineStatusProperty);
            set => this.SetValue(MachineStatusProperty, value);
        }

        #endregion

        #region Methods

        protected Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            this.MachineStatus = this.machineService.MachineStatus;

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
