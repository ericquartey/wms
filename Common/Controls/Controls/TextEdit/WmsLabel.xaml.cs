using System;
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

        public static readonly DependencyProperty OriginalTitleProperty = DependencyProperty.RegisterAttached(
                   nameof(OriginalTitle), typeof(string), typeof(WmsLabel), new UIPropertyMetadata());

        public static readonly DependencyProperty TitleProperty = DependencyProperty.RegisterAttached(
           nameof(Title), typeof(string), typeof(WmsLabel), new UIPropertyMetadata(OnTitleChanged));

        #endregion

        #region Constructors

        public WmsLabel()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public string OriginalTitle { get => (string)this.GetValue(OriginalTitleProperty); set => this.SetValue(OriginalTitleProperty, value); }

        public string Title { get => (string)this.GetValue(TitleProperty); set => this.SetValue(TitleProperty, value); }

        #endregion

        #region Methods

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsLabel wmsLabel && e.NewValue is string title)
            {
                if (wmsLabel.GridWmsLabel.Children.Count > 0)
                {
                    ((Label)wmsLabel.GridWmsLabel.Children[0]).Content = title;
                }
            }
        }

        #endregion
    }
}
