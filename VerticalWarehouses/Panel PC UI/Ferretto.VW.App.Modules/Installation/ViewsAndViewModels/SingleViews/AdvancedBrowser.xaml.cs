using System;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using Ferretto.VW.App.Services;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Installation.Views
{
    public partial class AdvancedBrowser : UserControl
    {
        #region Fields

        public static readonly DependencyProperty IsOpenProperty =
           DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(AdvancedBrowser), new PropertyMetadata(OnIsOpenChanged));

        public static readonly DependencyProperty UrlProperty =
                   DependencyProperty.Register(nameof(Url), typeof(string), typeof(AdvancedBrowser), new PropertyMetadata(OnUrlChanged));

        #endregion

        #region Constructors

        public AdvancedBrowser()
        {
            this.InitializeComponent();
            this.MyWebBrowser.DownloadHandler = new DownloadHandler();
        }

        #endregion

        #region Properties

        public bool IsOpen
        {
            get => (bool)this.GetValue(IsOpenProperty);
            set => this.SetValue(IsOpenProperty, value);
        }

        public string Url
        {
            get => (string)this.GetValue(UrlProperty);
            set => this.SetValue(UrlProperty, value);
        }

        #endregion

        #region Methods

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var istance = d as AdvancedBrowser;
            if ((bool)e.NewValue == false)
            {
                if (!string.IsNullOrEmpty(istance.Url))
                {
                    istance.MyWebBrowser.Address = istance.Url;
                }
            }
        }

        private static void OnUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var istance = d as AdvancedBrowser;
            if (!string.IsNullOrEmpty(e.NewValue.ToString()))
            {
                istance.MyWebBrowser.Address = e.NewValue.ToString();
            }
        }

        private void PpcButton_Click(object sender, RoutedEventArgs e)
        {
            this.MyWebBrowser.Reload();
        }

        private void PpcButton_TouchUp(object sender, System.Windows.Input.TouchEventArgs e)
        {
            this.MyWebBrowser.Reload();
        }

        #endregion
    }

    public class DownloadHandler : IDownloadHandler
    {
        #region Fields

        private readonly IEventAggregator eventAggregator = CommonServiceLocator.ServiceLocator.Current.GetInstance<IEventAggregator>();

        #endregion

        #region Events

        public event EventHandler<DownloadItem> OnBeforeDownloadFired;

        public event EventHandler<DownloadItem> OnDownloadUpdatedFired;

        #endregion

        #region Methods

        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            OnBeforeDownloadFired?.Invoke(this, downloadItem);

            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    callback.Continue(downloadItem.SuggestedFileName, showDialog: true);
                }
            }
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            OnDownloadUpdatedFired?.Invoke(this, downloadItem);

            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage($"Percentuale download: {downloadItem.PercentComplete}", Services.Models.NotificationSeverity.Info));
        }

        #endregion
    }
}
