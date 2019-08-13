using System.Windows;
using System.Windows.Controls;

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

            this.SourceUpdated += this.PpcStackedMenu_SourceUpdated;
        }

        #endregion

        #region Properties

        public Orientation Orientation
        {
            get => (Orientation)this.GetValue(OrientationProperty);
            set => this.SetValue(OrientationProperty, value);
        }

        #endregion

        #region Methods

        private void PpcStackedMenu_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            if (e.Source is NavigationMenuItem menuItem)
            {
            }
        }

        #endregion
    }
}
