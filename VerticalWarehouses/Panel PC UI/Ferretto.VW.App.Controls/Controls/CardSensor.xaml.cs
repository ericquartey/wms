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

        #endregion

        #region Constructors

        public CardSensor()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

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

        #endregion
    }
}
