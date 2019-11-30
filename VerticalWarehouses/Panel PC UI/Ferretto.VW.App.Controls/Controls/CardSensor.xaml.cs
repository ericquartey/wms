using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for CardSensor.xaml
    /// </summary>
    public partial class CardSensor : UserControl
    {
        #region Fields

        public static readonly DependencyProperty CardBadge1LabelProperty =
            DependencyProperty.Register(
                nameof(CardBadgeLabel1),
                typeof(string),
                typeof(CardSensor),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardBadge1Property =
            DependencyProperty.Register(
                nameof(CardBadge1),
                typeof(string),
                typeof(CardSensor),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardBadge2LabelProperty =
            DependencyProperty.Register(
                nameof(CardBadgeLabel2),
                typeof(string),
                typeof(CardSensor),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardBadge2Property =
            DependencyProperty.Register(
                nameof(CardBadge2),
                typeof(string),
                typeof(CardSensor),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardSensorLabel1Property =
            DependencyProperty.Register(
                nameof(CardSensorLabel1),
                typeof(string),
                typeof(CardSensor),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardSensorLabel2Property =
            DependencyProperty.Register(
                nameof(CardSensorLabel2),
                typeof(string),
                typeof(CardSensor),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardSensorLabel3Property =
            DependencyProperty.Register(
                nameof(CardSensorLabel3),
                typeof(string),
                typeof(CardSensor),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardTextProperty =
            DependencyProperty.Register(
                nameof(CardText),
                typeof(string),
                typeof(CardSensor),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardTypeProperty =
            DependencyProperty.Register(
                nameof(Type),
                typeof(CardType),
                typeof(CardSensor),
                new PropertyMetadata(CardType.Axis));

        public static readonly DependencyProperty CardValueProperty =
            DependencyProperty.Register(
                nameof(CardValue),
                typeof(string),
                typeof(CardSensor),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty Sensor1Property =
            DependencyProperty.Register(
                nameof(Sensor1),
                typeof(bool),
                typeof(CardSensor),
                new PropertyMetadata(false));

        public static readonly DependencyProperty Sensor2Property =
            DependencyProperty.Register(
                nameof(Sensor2),
                typeof(bool),
                typeof(CardSensor),
                new PropertyMetadata(false));

        public static readonly DependencyProperty Sensor3Property =
            DependencyProperty.Register(
                nameof(Sensor3),
                typeof(bool),
                typeof(CardSensor),
                new PropertyMetadata(false));

        #endregion

        #region Constructors

        public CardSensor()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Enums

        public enum CardType
        {
            Axis,

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

        public CardType Type
        {
            get => (CardType)this.GetValue(CardTypeProperty);
            set => this.SetValue(CardTypeProperty, value);
        }

        #endregion
    }
}
