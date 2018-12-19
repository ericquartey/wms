using System.Windows;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    public partial class WmsImage : UserControl
    {
        #region Fields

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
         nameof(Path), typeof(string), typeof(WmsImage), new PropertyMetadata(default(string), new PropertyChangedCallback(OnPathChanged)));

        #endregion Fields

        #region Constructors

        public WmsImage()
        {
            this.InitializeComponent();

            this.InnerImage.DataContext = new WmsImageViewModel();
        }

        #endregion Constructors

        #region Properties

        public string Path
        {
            get => (string)this.GetValue(PathProperty);
            set => this.SetValue(PathProperty, value);
        }

        #endregion Properties

        #region Methods

        private static void OnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsImage wmsImage && wmsImage.InnerImage.DataContext is WmsImageViewModel viewModel)
            {
                viewModel.RetrieveImage((string)e.NewValue);
            }
        }

        #endregion Methods
    }
}
