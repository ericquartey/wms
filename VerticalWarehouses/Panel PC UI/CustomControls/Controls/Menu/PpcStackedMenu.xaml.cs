using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.VW.App.Controls.Controls;

namespace Ferretto.VW.App.Controls
{
    public partial class PpcStackedMenu : ItemsControl
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

        public Orientation Orientation
        {
            get => (Orientation)this.GetValue(OrientationProperty);
            set
            {
                this.SetValue(OrientationProperty, value);
            }
        }

        #endregion
    }
}
