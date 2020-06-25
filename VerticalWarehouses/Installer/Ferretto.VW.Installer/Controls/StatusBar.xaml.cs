using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.Installer.Services;

#nullable enable

namespace Ferretto.VW.Installer.Controls
{
    public sealed partial class StatusBar : UserControl, IDisposable
    {
        #region Fields

        public static readonly DependencyProperty IsErrorProperty
            = DependencyProperty.Register(nameof(IsError), typeof(bool), typeof(StatusBar));

        public static readonly DependencyProperty MessageProperty
            = DependencyProperty.Register(nameof(Message), typeof(string), typeof(StatusBar));

        private bool isDisposed;

        private readonly INotificationService notificationService;

        private readonly PropertyChangedEventHandler propertyChangedHandler;

        #endregion

        #region Constructors

        public StatusBar()
        {
            this.InitializeComponent();

            this.InnerGrid.DataContext = this;

            this.propertyChangedHandler = new PropertyChangedEventHandler(this.NotificationService_PropertyChanged);
            this.notificationService = NotificationService.GetInstance();
            this.notificationService.PropertyChanged += this.propertyChangedHandler;
        }

        #endregion

        #region Properties

        public bool IsError
        {
            get => (bool)this.GetValue(IsErrorProperty);
            set => this.SetValue(IsErrorProperty, value);
        }

        public string? Message
        {
            get => (string?)this.GetValue(MessageProperty);
            set => this.SetValue(MessageProperty, value);
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.notificationService.PropertyChanged -= this.propertyChangedHandler;

            this.isDisposed = true;
        }

        
        private void NotificationService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(INotificationService.Message):
                    this.Message = this.notificationService.Message;
                    break;
                case nameof(INotificationService.IsError):
                    this.IsError = this.notificationService.IsError;
                    break;
            }
        }

        #endregion
    }
}
