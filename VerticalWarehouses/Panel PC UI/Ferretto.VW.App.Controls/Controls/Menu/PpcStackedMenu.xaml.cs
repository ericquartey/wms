using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Controls
{
    public partial class PpcStackedMenu
    {
        #region Fields

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(PpcStackedMenu));

        #endregion

        #region Constructors

        public PpcStackedMenu()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public Orientation Orientation
        {
            get => (Orientation)this.GetValue(OrientationProperty);
            set => this.SetValue(OrientationProperty, value);
        }

        #endregion
    }
}
