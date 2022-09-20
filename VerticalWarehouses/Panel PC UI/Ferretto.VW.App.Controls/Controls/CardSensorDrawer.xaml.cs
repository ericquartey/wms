using System.ComponentModel;
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
        public static readonly DependencyProperty NetWeightProperty =
           DependencyProperty.Register(nameof(NetWeight), typeof(double?), typeof(CardSensorDrawer));

        [Browsable(false)]
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(string), typeof(CardSensorDrawer));

        private IEventAggregator eventAggregator;

        private IMachineService machineService;

        private SubscriptionToken machineStatusChangesToken;

        private IThemeService themeService;

        #endregion

        #region Constructors

        public CardSensorDrawer()
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

        public double? NetWeight
        {
            get => (double?)this.GetValue(NetWeightProperty);
            set => this.SetValue(NetWeightProperty, value);
        }

        public string Position
        {
            get => (string)this.GetValue(PositionProperty);
            set => this.SetValue(PositionProperty, value);
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
            this.machineService = null;
        }

        protected void OnAppeared()
        {
            this.SubscribeToEvents();

            this.OnDataRefresh();
        }

        protected void OnDataRefresh()
        {
            var lu = this.machineService.MachineStatus.EmbarkedLoadingUnit;
            this.Position = Localized.Get("SensorCard.Elevator");
            if (lu is null)
            {
                this.Position = Localized.Get("SensorCard.Position");
                lu = this.machineService.MachineStatus.ElevatorPositionLoadingUnit;
            }
            if (lu is null)
            {
                this.GrossWeight = null;
                this.NetWeight = null;
                this.Position = null;
                this.Height = null;
            }
            else
            {
                this.GrossWeight = lu.GrossWeight;
                this.NetWeight = lu.NetWeight;
                this.Height = lu.Height;
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
