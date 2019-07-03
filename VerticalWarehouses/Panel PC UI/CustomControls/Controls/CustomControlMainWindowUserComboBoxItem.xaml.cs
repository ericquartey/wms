﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ferretto.VW.CustomControls.Controls
{
    /// <summary>
    /// Interaction logic for CustomControlMainWindowUserComboBoxItem.xaml
    /// </summary>
    public partial class CustomControlMainWindowUserComboBoxItem : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty CustomCommandProperty = DependencyProperty.Register(
            "CustomCommand",
            typeof(ICommand),
            typeof(CustomControlMainWindowUserComboBoxItem));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "LabelText",
            typeof(string),
            typeof(CustomControlMainWindowUserComboBoxItem),
            new PropertyMetadata(string.Empty));

        #endregion

        #region Constructors

        public CustomControlMainWindowUserComboBoxItem()
        {
            this.InitializeComponent();
            var customControlMainWindowUserComboBoxItem = this;
            this.LayoutRoot.DataContext = customControlMainWindowUserComboBoxItem;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public ICommand CustomCommand
        {
            get => (ICommand)this.GetValue(CustomCommandProperty);
            set
            {
                this.SetValue(CustomCommandProperty, value);
                this.RaisePropertyChanged(nameof(this.CustomCommand));
            }
        }

        public string LabelText
        {
            get => (string)this.GetValue(LabelProperty);
            set
            {
                this.SetValue(LabelProperty, value);
                this.RaisePropertyChanged(nameof(this.LabelText));
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
