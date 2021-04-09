using System;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Modules.Installation.Views
{
    public partial class BrowserView : UserControl
    {
        #region Fields

        public static readonly DependencyProperty UrlProperty =
           DependencyProperty.Register(nameof(Url), typeof(string), typeof(BrowserView), new PropertyMetadata(OnUrlChanged));

        #endregion

        #region Constructors

        public BrowserView()
        {
            this.InitializeComponent();

            Instance = this;
        }

        #endregion

        #region Properties

        public static BrowserView Instance { get; private set; }

        public string Url
        {
            get => (string)this.GetValue(UrlProperty);
            set => this.SetValue(UrlProperty, value);
        }

        #endregion

        #region Methods

        private static void OnUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.NewValue.ToString()))
            {
                Instance.MyWebBrowser.Navigate(("http://" + e.NewValue.ToString()));
                //Instance.MyWebBrowser.Refresh();
            }
        }

        private void PpcButton_Click(object sender, RoutedEventArgs e)
        {
            this.MyWebBrowser.Refresh();
        }

        #endregion
    }
}
