using System.Windows;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    /// <summary>
    /// Interaction logic for WmsLabel.xaml
    /// </summary>
    public partial class WmsLabel : UserControl
    {
        #region Fields

        public static readonly DependencyProperty TitleProperty = DependencyProperty.RegisterAttached(
                   nameof(Title), typeof(string), typeof(WmsLabel), new UIPropertyMetadata());

        #endregion

        #region Constructors

        public WmsLabel()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public string Title { get => (string)this.GetValue(TitleProperty); set => this.SetValue(TitleProperty, value); }

        #endregion
    }
}
