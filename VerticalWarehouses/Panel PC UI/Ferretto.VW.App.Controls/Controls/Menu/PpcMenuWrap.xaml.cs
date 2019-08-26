using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Controls
{
    public partial class PpcMenuWrap : ItemsControl
    {
        #region Fields

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(PpcMenuWrap));

        #endregion

        #region Constructors

        public PpcMenuWrap()
        {
            this.InitializeComponent();
        }

        public Orientation Orientation
        {
            get => (Orientation)this.GetValue(OrientationProperty);
            set => this.SetValue(OrientationProperty, value);
        }

        #endregion
    }
}
