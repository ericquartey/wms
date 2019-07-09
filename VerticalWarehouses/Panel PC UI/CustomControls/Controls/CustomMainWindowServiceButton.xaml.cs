﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for CustomMainWindowServiceButton.xaml
    /// </summary>
    public partial class CustomMainWindowServiceButton : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty ContentTextProperty = DependencyProperty.Register("ContentText", typeof(string), typeof(CustomMainWindowServiceButton));

        public static readonly DependencyProperty CustomCommandProperty = DependencyProperty.Register("CustomCommand", typeof(ICommand), typeof(CustomMainWindowServiceButton));

        #endregion

        #region Constructors

        public CustomMainWindowServiceButton()
        {
            this.InitializeComponent();
            var customMainWindowServiceButton = this;
            this.LayoutRoot.DataContext = customMainWindowServiceButton;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public string ContentText
        {
            get => (string)this.GetValue(ContentTextProperty);
            set
            {
                this.SetValue(ContentTextProperty, value);
                this.RaisePropertyChanged(nameof(this.ContentText));
            }
        }

        public ICommand CustomCommand
        {
            get => (ICommand)this.GetValue(CustomCommandProperty);
            set
            {
                this.SetValue(CustomCommandProperty, value);
                this.RaisePropertyChanged(nameof(this.CustomCommand));
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
