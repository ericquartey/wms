using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for CardSensor.xaml
    /// </summary>
    public partial class CardSensor : UserControl
    {
        #region Fields

        public static readonly DependencyProperty CardBadge1LabelProperty =
            DependencyProperty.Register(nameof(CardBadgeLabel1), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardBadge1Property =
            DependencyProperty.Register(nameof(CardBadge1), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardBadge2LabelProperty =
            DependencyProperty.Register(nameof(CardBadgeLabel2), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardBadge2Property =
            DependencyProperty.Register(nameof(CardBadge2), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardBadge3LabelProperty =
            DependencyProperty.Register(nameof(CardBadgeLabel3), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardBadge3Property =
            DependencyProperty.Register(nameof(CardBadge3), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardBayPositionProperty =
            DependencyProperty.Register(nameof(CardBayPosition), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardDescriptionProperty =
            DependencyProperty.Register(nameof(CardDescription), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardLuDownProperty =
            DependencyProperty.Register(nameof(CardLuDown), typeof(LoadingUnit), typeof(CardSensor));

        public static readonly DependencyProperty CardSensorLabel1Property =
            DependencyProperty.Register(nameof(CardSensorLabel1), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardSensorLabel2Property =
            DependencyProperty.Register(nameof(CardSensorLabel2), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardSensorLabel3Property =
            DependencyProperty.Register(nameof(CardSensorLabel3), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardSensorLabel4Property =
            DependencyProperty.Register(nameof(CardSensorLabel4), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardSensorLabel5Property =
            DependencyProperty.Register(nameof(CardSensorLabel5), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardSensorLabel6Property =
            DependencyProperty.Register(nameof(CardSensorLabel6), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardTextProperty =
            DependencyProperty.Register(nameof(CardText), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardTypeProperty =
            DependencyProperty.Register(nameof(Type), typeof(CardType), typeof(CardSensor), new PropertyMetadata(CardType.Axis));

        public static readonly DependencyProperty CardUpLuProperty =
            DependencyProperty.Register(nameof(CardLuUp), typeof(LoadingUnit), typeof(CardSensor));

        public static readonly DependencyProperty CardValueProperty =
            DependencyProperty.Register(nameof(CardValue), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LuHeightProperty =
            DependencyProperty.Register(nameof(LuHeight), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LuWeightProperty =
            DependencyProperty.Register(nameof(LuWeight), typeof(string), typeof(CardSensor), new PropertyMetadata(string.Empty));

        [Browsable(false)]
        public static readonly DependencyProperty MachineServiceProperty =
            DependencyProperty.Register(nameof(MachineService), typeof(IMachineService), typeof(CardSensor));

        public static readonly DependencyProperty Sensor1Property =
            DependencyProperty.Register(nameof(Sensor1), typeof(bool), typeof(CardSensor), new PropertyMetadata(false));

        public static readonly DependencyProperty Sensor2Property =
            DependencyProperty.Register(nameof(Sensor2), typeof(bool), typeof(CardSensor), new PropertyMetadata(false));

        public static readonly DependencyProperty Sensor3Property =
            DependencyProperty.Register(nameof(Sensor3), typeof(bool), typeof(CardSensor), new PropertyMetadata(false));

        public static readonly DependencyProperty Sensor4Property =
            DependencyProperty.Register(nameof(Sensor4), typeof(bool), typeof(CardSensor), new PropertyMetadata(false));

        public static readonly DependencyProperty Sensor5Property =
            DependencyProperty.Register(nameof(Sensor5), typeof(bool), typeof(CardSensor), new PropertyMetadata(false));

        public static readonly DependencyProperty Sensor6Property =
            DependencyProperty.Register(nameof(Sensor6), typeof(bool), typeof(CardSensor), new PropertyMetadata(false));

        #endregion

        #region Constructors

        public CardSensor()
        {
            this.InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.MachineService = ServiceLocator.Current.GetInstance<IMachineService>();

            this.HasCarousel = this.MachineService.HasCarousel;

            this.HasBayWithInverter = this.MachineService.HasBayWithInverter;
        }

        #endregion

        #region Enums

        public enum CardType
        {
            Axis,

            Bay,

            BED,

            Drawer,

            Position,

            Shutter,
        }

        #endregion

        #region Properties

        public string CardBadge1
        {
            get => (string)this.GetValue(CardBadge1Property);
            set => this.SetValue(CardBadge1Property, value);
        }

        public string CardBadge2
        {
            get => (string)this.GetValue(CardBadge2Property);
            set => this.SetValue(CardBadge2Property, value);
        }

        public string CardBadge3
        {
            get => (string)this.GetValue(CardBadge3Property);
            set => this.SetValue(CardBadge3Property, value);
        }

        public string CardBadgeLabel1
        {
            get => (string)this.GetValue(CardBadge1LabelProperty);
            set => this.SetValue(CardBadge1LabelProperty, value);
        }

        public string CardBadgeLabel2
        {
            get => (string)this.GetValue(CardBadge2LabelProperty);
            set => this.SetValue(CardBadge2LabelProperty, value);
        }

        public string CardBadgeLabel3
        {
            get => (string)this.GetValue(CardBadge3LabelProperty);
            set => this.SetValue(CardBadge3LabelProperty, value);
        }

        public string CardBayPosition
        {
            get => (string)this.GetValue(CardBayPositionProperty);
            set => this.SetValue(CardBayPositionProperty, value);
        }

        public string CardDescription
        {
            get => (string)this.GetValue(CardDescriptionProperty);
            set => this.SetValue(CardDescriptionProperty, value);
        }

        public LoadingUnit CardLuDown
        {
            get => (LoadingUnit)this.GetValue(CardLuDownProperty);
            set => this.SetValue(CardLuDownProperty, value);
        }

        public LoadingUnit CardLuUp
        {
            get => (LoadingUnit)this.GetValue(CardUpLuProperty);
            set => this.SetValue(CardUpLuProperty, value);
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

        public string CardText
        {
            get => (string)this.GetValue(CardTextProperty);
            set => this.SetValue(CardTextProperty, value);
        }

        public string CardValue
        {
            get => (string)this.GetValue(CardValueProperty);
            set => this.SetValue(CardValueProperty, value);
        }

        public bool HasBayWithInverter { get; set; }

        public bool HasCarousel { get; set; }

        public string LuHeight
        {
            get => (string)this.GetValue(LuHeightProperty);
            set => this.SetValue(LuHeightProperty, value);
        }

        public string LuWeight
        {
            get => (string)this.GetValue(LuWeightProperty);
            set => this.SetValue(LuWeightProperty, value);
        }

        public IMachineService MachineService
        {
            get => (IMachineService)this.GetValue(MachineServiceProperty);
            set => this.SetValue(MachineServiceProperty, value);
        }

        public bool Sensor1
        {
            get => (bool)this.GetValue(Sensor1Property);
            set => this.SetValue(Sensor1Property, value);
        }

        public bool Sensor2
        {
            get => (bool)this.GetValue(Sensor1Property);
            set => this.SetValue(Sensor1Property, value);
        }

        public bool Sensor3
        {
            get => (bool)this.GetValue(Sensor1Property);
            set => this.SetValue(Sensor1Property, value);
        }

        public bool Sensor4
        {
            get => (bool)this.GetValue(Sensor1Property);
            set => this.SetValue(Sensor1Property, value);
        }

        public bool Sensor5
        {
            get => (bool)this.GetValue(Sensor1Property);
            set => this.SetValue(Sensor1Property, value);
        }

        public bool Sensor6
        {
            get => (bool)this.GetValue(Sensor1Property);
            set => this.SetValue(Sensor1Property, value);
        }

        public CardType Type
        {
            get => (CardType)this.GetValue(CardTypeProperty);
            set => this.SetValue(CardTypeProperty, value);
        }

        #endregion
    }
}
