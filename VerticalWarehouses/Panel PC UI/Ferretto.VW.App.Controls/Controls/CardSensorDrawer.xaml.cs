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
    /// Interaction logic for CardSensorDrawer
    /// </summary>
    public partial class CardSensorDrawer : UserControl
    {
        #region Fields

        [Browsable(false)]
        public static readonly DependencyProperty GrossWeightProperty =
            DependencyProperty.Register(nameof(GrossWeight), typeof(double?), typeof(CardSensorDrawer));

        [Browsable(false)]
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register(nameof(Height), typeof(double?), typeof(CardSensorDrawer));

        [Browsable(false)]
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(string), typeof(CardSensorDrawer));

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineService machineService;

        private SubscriptionToken machineStatusChangesToken;

        #endregion

        #region Constructors

        public CardSensorDrawer()
        {
            this.InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.Loaded += (s, e) =>
            {
                LoadingUnit lu = this.machineService.MachineStatus.EmbarkedLoadingUnit;
                this.Position = "Elevatore";
                if (lu is null)
                {
                    this.Position = "Posizione";
                    lu = this.machineService.MachineStatus.ElevatorPositionLoadingUnit;
                }
                if (lu is null)
                {
                    this.GrossWeight = null;
                    this.Position = null;
                    this.Height = null;
                }
                else
                {
                    this.GrossWeight = lu.GrossWeight;
                    this.Height = lu.Height;
                }
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

        public double? GrossWeight
        {
            get => (double?)this.GetValue(GrossWeightProperty);
            set => this.SetValue(GrossWeightProperty, value);
        }

        public double? Height
        {
            get => (double?)this.GetValue(HeightProperty);
            set => this.SetValue(HeightProperty, value);
        }

        public string Position
        {
            get => (string)this.GetValue(PositionProperty);
            set => this.SetValue(PositionProperty, value);
        }

        #endregion

        #region Methods

        protected Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            LoadingUnit lu = e.MachineStatus.EmbarkedLoadingUnit;
            this.Position = "Elevatore";
            if (lu is null)
            {
                this.Position = "Posizione";
                lu = e.MachineStatus.ElevatorPositionLoadingUnit;
            }
            if (lu is null)
            {
                this.GrossWeight = null;
                this.Position = null;
                this.Height = null;
            }
            else
            {
                this.GrossWeight = lu.GrossWeight;
                this.Height = lu.Height;
            }

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
