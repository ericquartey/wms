﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for CustomControlLoggedUser.xaml
    /// </summary>
    public partial class CustomControlLoggedUser : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty IsPopupOpenProperty = DependencyProperty.Register("IsPopupOpen", typeof(bool), typeof(CustomControlLoggedUser));

        public static readonly DependencyProperty LogOffCommandProperty = DependencyProperty.Register("LogOffCommand", typeof(ICommand), typeof(CustomControlLoggedUser));

        public static readonly DependencyProperty OpenClosePopupCommandProperty = DependencyProperty.Register("OpenClosePopupCommand", typeof(ICommand), typeof(CustomControlLoggedUser));

        public static readonly DependencyProperty UserTextProperty = DependencyProperty.Register("UserText", typeof(string), typeof(CustomControlLoggedUser));

        #endregion

        #region Constructors

        public CustomControlLoggedUser()
        {
            this.InitializeComponent();
            var customControlLoggedUser = this;
            this.LayoutRoot.DataContext = customControlLoggedUser;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public bool IsPopupOpen
        {
            get => (bool)this.GetValue(IsPopupOpenProperty);
            set
            {
                this.SetValue(IsPopupOpenProperty, value);
                this.RaisePropertyChanged(nameof(this.IsPopupOpen));
            }
        }

        public ICommand LogOffCommand
        {
            get => (ICommand)this.GetValue(LogOffCommandProperty);
            set
            {
                this.SetValue(LogOffCommandProperty, value);
                this.RaisePropertyChanged(nameof(this.LogOffCommand));
            }
        }

        public ICommand OpenClosePopupCommand
        {
            get => (ICommand)this.GetValue(OpenClosePopupCommandProperty);
            set
            {
                this.SetValue(OpenClosePopupCommandProperty, value);
                this.RaisePropertyChanged(nameof(this.OpenClosePopupCommand));
            }
        }

        public string UserText
        {
            get => (string)this.GetValue(UserTextProperty);
            set
            {
                this.SetValue(UserTextProperty, value);
                this.RaisePropertyChanged(nameof(this.UserText));
            }
        }

        #endregion

        #region Methods

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
