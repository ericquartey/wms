using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ferretto.VW.CustomControls.Controls
{
    public partial class CustomControlComboBox : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty CurrentSelectionProperty = DependencyProperty.Register("CurrentSelection", typeof(int), typeof(CustomControlComboBox));
        public static readonly DependencyProperty CustomBorderBrushProperty = DependencyProperty.Register("CustomBorderBrush", typeof(SolidColorBrush), typeof(CustomControlComboBox));
        public static readonly DependencyProperty CustomComboBoxStateProperty = DependencyProperty.Register("CustomComboBoxState", typeof(bool), typeof(CustomControlComboBox));
        public static readonly DependencyProperty Item1TextProperty = DependencyProperty.Register("Item1Text", typeof(string), typeof(CustomControlComboBox), new PropertyMetadata(""));
        public static readonly DependencyProperty Item2TextProperty = DependencyProperty.Register("Item2Text", typeof(string), typeof(CustomControlComboBox), new PropertyMetadata(""));

        #endregion Fields

        #region Constructors

        public CustomControlComboBox()
        {
            this.InitializeComponent();
            this.LayoutRoot.DataContext = this;
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public Int32 CurrentSelection
        {
            get => (Int32)this.GetValue(CurrentSelectionProperty);
            set { this.SetValue(CurrentSelectionProperty, value); this.RaisePropertyChanged("CurrentSelection"); }
        }

        public SolidColorBrush CustomBorderBrush
        {
            get => (SolidColorBrush)this.GetValue(CustomBorderBrushProperty);
            set { this.SetValue(CustomBorderBrushProperty, value); this.RaisePropertyChanged("CustomBorderBrush"); }
        }

        public Boolean CustomComboBoxState
        {
            get => (Boolean)this.GetValue(CustomComboBoxStateProperty);
            set { this.SetValue(CustomComboBoxStateProperty, value); this.RaisePropertyChanged("CustomComboBoxState"); }
        }

        public String Item1Text
        {
            get => (String)this.GetValue(Item1TextProperty);
            set { this.SetValue(Item1TextProperty, value); this.RaisePropertyChanged("Item1Text"); }
        }

        public String Item2Text
        {
            get => (String)this.GetValue(Item2TextProperty);
            set { this.SetValue(Item2TextProperty, value); this.RaisePropertyChanged("Item2Text"); }
        }

        #endregion Properties

        #region Methods

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion Methods
    }
}
