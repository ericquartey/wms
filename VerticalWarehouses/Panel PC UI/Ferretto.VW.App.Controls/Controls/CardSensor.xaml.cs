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

        public static readonly DependencyProperty CardLabelSensor1Property =
            DependencyProperty.Register(
                nameof(CardLabelSensor1),
                typeof(string),
                typeof(CardSensor),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardLabelSensor2Property =
            DependencyProperty.Register(
                nameof(CardLabelSensor2),
                typeof(string),
                typeof(CardSensor),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardLabelSensor3Property =
            DependencyProperty.Register(
                nameof(CardLabelSensor3),
                typeof(string),
                typeof(CardSensor),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardTextProperty =
            DependencyProperty.Register(
                nameof(CardText),
                typeof(string),
                typeof(CardSensor),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardValueProperty =
            DependencyProperty.Register(
                nameof(CardValue),
                typeof(string),
                typeof(CardSensor),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty HasCardBadge1Property =
            DependencyProperty.Register(
                nameof(HasCardBadge1),
                typeof(bool),
                typeof(CardSensor),
                new PropertyMetadata(false));

        public static readonly DependencyProperty HasCardBadgeProperty =
            DependencyProperty.Register(
                nameof(HasCardBadge),
                typeof(bool),
                typeof(CardSensor),
                new PropertyMetadata(false));

        public static readonly DependencyProperty HasCardValueProperty =
            DependencyProperty.Register(
                nameof(HasCardValue),
                typeof(bool),
                typeof(CardSensor),
                new PropertyMetadata(false));

        public static readonly DependencyProperty HasWithBadge1Property =
            DependencyProperty.Register(
                nameof(HasWithBadge1),
                typeof(string),
                typeof(CardSensor),
                new PropertyMetadata("XXX"));

        public static readonly DependencyProperty HasWithBadge2Property =
            DependencyProperty.Register(
                nameof(HasWithBadge2),
                typeof(string),
                typeof(CardSensor),
                new PropertyMetadata("XXX"));

        #endregion

        #region Constructors

        public CardSensor()
        {
            this.InitializeComponent();
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

        public string CardLabelSensor1
        {
            get => (string)this.GetValue(CardLabelSensor1Property);
            set => this.SetValue(CardLabelSensor1Property, value);
        }

        public string CardLabelSensor2
        {
            get => (string)this.GetValue(CardLabelSensor2Property);
            set => this.SetValue(CardLabelSensor2Property, value);
        }

        public string CardLabelSensor3
        {
            get => (string)this.GetValue(CardLabelSensor3Property);
            set => this.SetValue(CardLabelSensor3Property, value);
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

        public bool HasCardBadge
        {
            get => !string.IsNullOrEmpty(this.CardBadge1) &&
                   !string.IsNullOrEmpty(this.CardBadge2) &&
                   !string.IsNullOrEmpty(this.CardBadgeLabel1) &&
                   !string.IsNullOrEmpty(this.CardBadgeLabel2);
        }

        public bool HasCardBadge1
        {
            get => !string.IsNullOrEmpty(this.CardBadge1);
        }

        public bool HasCardValue
        {
            get => !string.IsNullOrEmpty(this.CardValue);
        }

        public string HasWithBadge1
        {
            get => (string)this.GetValue(HasWithBadge1Property);
            set => this.SetValue(HasWithBadge1Property, value);
        }

        public string HasWithBadge2
        {
            get => (string)this.GetValue(HasWithBadge2Property);
            set => this.SetValue(HasWithBadge2Property, value);
        }

        #endregion
    }
}
