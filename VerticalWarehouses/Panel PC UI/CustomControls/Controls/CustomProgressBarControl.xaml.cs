﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.CustomControls.Controls
{
    /// <summary>
    /// Interaction logic for CustomLabelTextBlockControl.xaml
    /// </summary>
    public partial class CustomProgressBarControl : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty ContentTextProperty = DependencyProperty.Register("ContentText", typeof(string), typeof(CustomProgressBarControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("LabelText", typeof(string), typeof(CustomProgressBarControl), new PropertyMetadata(string.Empty));

        #endregion

        #region Constructors

        public CustomProgressBarControl()
        {
            this.InitializeComponent();
            var customProgressBarControl = this;
            /// this.LayoutRoot.DataContext = customProgressBarControl;
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
