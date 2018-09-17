using System.Windows;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    public partial class Image : UserControl
    {
        #region Fields

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
         nameof(Path), typeof(string), typeof(Image), new PropertyMetadata(default(string), new PropertyChangedCallback(OnPathChanged)));

        #endregion Fields

        #region Constructors

        public Image()
        {
            this.InitializeComponent();

            this.DataContext = new ImageViewModel();
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
            if (d is Image image
                &&
                image.DataContext is ImageViewModel viewModel
                &&
                e.NewValue is string imagePath
                &&
                string.IsNullOrWhiteSpace(imagePath) == false)
            {
                viewModel.RetrieveImage(imagePath);
            }
        }

        #endregion Methods
    }
}
