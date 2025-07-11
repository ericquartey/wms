﻿using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Prism.Events;
using static Ferretto.VW.App.Controls.Controls.CardSensor;

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
        public static readonly DependencyProperty CardSensorLabel4Property =
            DependencyProperty.Register(nameof(CardSensorLabel4), typeof(string), typeof(CardSensorBay));

        [Browsable(false)]
        public static readonly DependencyProperty CardSensorLabel5Property =
            DependencyProperty.Register(nameof(CardSensorLabel5), typeof(string), typeof(CardSensorBay));

        [Browsable(false)]
        public static readonly DependencyProperty CardSensorLabel6Property =
            DependencyProperty.Register(nameof(CardSensorLabel6), typeof(string), typeof(CardSensorBay));

        [Browsable(false)]
        public static readonly DependencyProperty CardTypeProperty =
            DependencyProperty.Register(nameof(Type), typeof(CardType), typeof(CardSensorBay));

        [Browsable(false)]
        public static readonly DependencyProperty MachineStatusProperty =
            DependencyProperty.Register(nameof(MachineStatus), typeof(App.Services.Models.MachineStatus), typeof(CardSensorBay));

        [Browsable(false)]
        public static readonly DependencyProperty Sensor2Property =
            DependencyProperty.Register(nameof(Sensor2), typeof(bool), typeof(CardSensorBay));

        [Browsable(false)]
        public static readonly DependencyProperty Sensor3Property =
            DependencyProperty.Register(nameof(Sensor3), typeof(bool), typeof(CardSensorBay));

        [Browsable(false)]
        public static readonly DependencyProperty Sensor4Property =
            DependencyProperty.Register(nameof(Sensor4), typeof(bool), typeof(CardSensorBay));

        [Browsable(false)]
        public static readonly DependencyProperty Sensor5Property =
            DependencyProperty.Register(nameof(Sensor5), typeof(bool), typeof(CardSensorBay));

        [Browsable(false)]
        public static readonly DependencyProperty SensorsServiceProperty =
            DependencyProperty.Register(nameof(SensorsService), typeof(ISensorsService), typeof(CardSensorBay));

        private IEventAggregator eventAggregator;

        private IMachineService machineService;

        private SubscriptionToken machineStatusChangesToken;

        private ISensorsService sensorsService;

        private IThemeService themeService;

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

        public string CardSensorLabel4
        {
            get => (string)this.GetValue(CardSensorLabel4Property);
            set => this.SetValue(CardSensorLabel4Property, value);
        }

        public string CardSensorLabel5
        {
            get => (string)this.GetValue(CardSensorLabel5Property);
            set => this.SetValue(CardSensorLabel5Property, value);
        }

        public string CardSensorLabel6
        {
            get => (string)this.GetValue(CardSensorLabel6Property);
            set => this.SetValue(CardSensorLabel6Property, value);
        }

        public App.Services.Models.MachineStatus MachineStatus
        {
            get => (App.Services.Models.MachineStatus)this.GetValue(MachineStatusProperty);
            set => this.SetValue(MachineStatusProperty, value);
        }

        public bool Sensor2
        {
            get => (bool)this.GetValue(Sensor2Property);
            set => this.SetValue(Sensor2Property, value);
        }

        public bool Sensor3
        {
            get => (bool)this.GetValue(Sensor3Property);
            set => this.SetValue(Sensor3Property, value);
        }

        public bool Sensor4
        {
            get => (bool)this.GetValue(Sensor4Property);
            set => this.SetValue(Sensor4Property, value);
        }

        public bool Sensor5
        {
            get => (bool)this.GetValue(Sensor5Property);
            set => this.SetValue(Sensor5Property, value);
        }

        public ISensorsService SensorsService
        {
            get => (ISensorsService)this.GetValue(SensorsServiceProperty);
            set => this.SetValue(SensorsServiceProperty, value);
        }

        public CardType Type
        {
            get => (CardType)this.GetValue(CardTypeProperty);
            set => this.SetValue(CardTypeProperty, value);
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
            this.Type = CardType.Bay;

            if (this.machineService.HasBayWithInverter)
            {
                this.CardSensorLabel1 = Localized.Get("SensorCard.Zero");
            }
            else
            {
                this.CardSensorLabel1 = string.Empty;
            }

            if (this.machineService.Bay.IsDouble && this.machineService.Bay.IsExternal)
            {
                if (this.machineService.Bay.Positions.Any(p => p.IsUpper && p.IsBlocked))
                {
                    this.CardSensorLabel4 = Localized.Get("SensorCard.InternalBlock");
                    this.CardSensorLabel2 = Localized.Get("SensorCard.ExternalBlock");
                }
                else
                {
                    this.CardSensorLabel4 = Localized.Get("SensorCard.Internal");
                    this.CardSensorLabel2 = Localized.Get("SensorCard.External");
                }
                if (this.machineService.Bay.Positions.Any(p => !p.IsUpper && p.IsBlocked))
                {
                    this.CardSensorLabel3 = Localized.Get("SensorCard.ExternalBlock");
                    this.CardSensorLabel5 = Localized.Get("SensorCard.InternalBlock");
                }
                else
                {
                    this.CardSensorLabel3 = Localized.Get("SensorCard.External");
                    this.CardSensorLabel5 = Localized.Get("SensorCard.Internal");
                }

                this.CardSensorLabel6 = Localized.Get("SensorCard.Zero");

                this.Sensor4 = this.sensorsService.BEDInternalBayTop;
                this.Sensor5 = this.sensorsService.BEDInternalBayBottom;
                this.Sensor2 = this.sensorsService.BEDExternalBayTop;
                this.Sensor3 = this.sensorsService.BEDExternalBayBottom;
                this.Type = CardType.BED;
            }
            else if (this.machineService.Bay.IsDouble)
            {
                if (this.machineService.Bay.Positions.Any(p => p.IsUpper && p.IsBlocked))
                {
                    this.CardSensorLabel2 = Localized.Get("OperatorApp.UpBlock");
                }
                else
                {
                    this.CardSensorLabel2 = Localized.Get("OperatorApp.Up");
                }

                if (this.machineService.Bay.Positions.Any(p => !p.IsUpper && p.IsBlocked))
                {
                    this.CardSensorLabel3 = Localized.Get("OperatorApp.DownBlock");
                }
                else
                {
                    this.CardSensorLabel3 = Localized.Get("OperatorApp.Down");
                }

                this.Sensor2 = this.sensorsService.IsLoadingUnitInBay;
                this.Sensor3 = this.sensorsService.IsLoadingUnitInMiddleBottomBay;
            }
            else
            {
                if (this.machineService.BayFirstPositionIsUpper)
                {
                    this.CardSensorLabel2 = Localized.Get("OperatorApp.Bay");
                    this.Sensor2 = this.sensorsService.IsLoadingUnitInBay;
                    if (this.machineService.Bay.IsTelescopic)
                    {
                        this.CardSensorLabel3 = Localized.Get("OperatorApp.Telescopic");
                        this.Sensor3 = this.sensorsService.BayTrolleyOption;
                    }
                    else
                    {
                        this.CardSensorLabel3 = null;
                    }
                }
                else
                {
                    this.CardSensorLabel2 = null;
                    this.CardSensorLabel3 = Localized.Get("OperatorApp.Bay");

                    this.Sensor3 = this.sensorsService.IsLoadingUnitInMiddleBottomBay;
                }
                if (this.machineService.Bays.Any(f => f.IsExternal))
                {
                    this.CardSensorLabel3 = Localized.Get("InstallationApp.ExternalBayShort");
                    this.CardSensorLabel2 = Localized.Get("InstallationApp.InternalBayShort");

                    this.Sensor3 = this.sensorsService.IsLoadingUnitInBay;
                    this.Sensor2 = this.sensorsService.IsLoadingUnitInMiddleBottomBay;
                }
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
