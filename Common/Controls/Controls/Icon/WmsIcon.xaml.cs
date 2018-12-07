using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ferretto.Common.Controls
{
    public partial class WmsIcon : UserControl
    {
        #region Fields

        public static readonly DependencyProperty ColorizeBrushProperty = DependencyProperty.Register(
        nameof(ColorizeBrush), typeof(SolidColorBrush), typeof(WmsIcon), new PropertyMetadata(default(SolidColorBrush), new PropertyChangedCallback(OnColorizeBrushChanged)));

        public static readonly DependencyProperty SymbolNameProperty = DependencyProperty.Register(
                 nameof(SymbolName), typeof(string), typeof(WmsIcon), new PropertyMetadata(default(string), new PropertyChangedCallback(OnSymbolNameChanged)));

        #endregion Fields

        #region Constructors

        public WmsIcon()
        {
            this.InitializeComponent();

            this.InnerImage.DataContext = new WmsIconViewModel();
        }

        #endregion Constructors

        #region Properties

        public SolidColorBrush ColorizeBrush
        {
            get => (SolidColorBrush)this.GetValue(ColorizeBrushProperty);
            set => this.SetValue(ColorizeBrushProperty, value);
        }

        public string SymbolName
        {
            get => (string)this.GetValue(SymbolNameProperty);
            set => this.SetValue(SymbolNameProperty, value);
        }

        #endregion Properties

        #region Methods

        private static void OnColorizeBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsIcon wmsIcon && wmsIcon.InnerImage.DataContext is WmsIconViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine($"OnColorizeBrushChanged: {wmsIcon.ColorizeBrush} - {wmsIcon.SymbolName}");
                viewModel.ColorizeImage((SolidColorBrush)e.NewValue);
            }
        }

        private static void OnSymbolNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsIcon wmsIcon && wmsIcon.InnerImage.DataContext is WmsIconViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine($"OnSymbolNameChanged: {wmsIcon.ColorizeBrush} - {wmsIcon.SymbolName}");
                viewModel.RetrieveImage((string)e.NewValue);
            }
        }

        #endregion Methods
    }
}
