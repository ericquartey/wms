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
    /// Interaction logic for PpcCheckBox.xaml
    /// </summary>
    public partial class PpcCheckBox : UserControl
    {
        #region Fields

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool?),
                typeof(PpcCheckBox),
                new FrameworkPropertyMetadata(
                    false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    null, null, false, UpdateSourceTrigger.PropertyChanged
                ));

        public static readonly DependencyProperty IsThreeStateProperty =
            DependencyProperty.Register(
                nameof(IsThreeState),
                typeof(bool),
                typeof(PpcCheckBox),
                new PropertyMetadata(false));

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register(
                nameof(LabelText),
                typeof(string),
                typeof(PpcCheckBox),
                new PropertyMetadata(string.Empty));

        #endregion

        #region Constructors

        public PpcCheckBox()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public bool? IsChecked
        {
            get => (bool?)this.GetValue(IsCheckedProperty);
            set => this.SetValue(IsCheckedProperty, value);
        }

        public bool IsThreeState
        {
            get => (bool)this.GetValue(IsThreeStateProperty);
            set => this.SetValue(IsThreeStateProperty, value);
        }

        public string LabelText
        {
            get => (string)this.GetValue(LabelTextProperty);
            set => this.SetValue(LabelTextProperty, value);
        }

        #endregion
    }
}
