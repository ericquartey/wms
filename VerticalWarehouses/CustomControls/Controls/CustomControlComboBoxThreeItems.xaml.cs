using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ferretto.VW.CustomControls.Controls
{
    public partial class CustomControlComboBoxThreeItems : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty CurrentSelectionProperty = DependencyProperty.Register("CurrentSelection", typeof(int), typeof(CustomControlComboBoxThreeItems));
        public static readonly DependencyProperty CustomBorderBrushProperty = DependencyProperty.Register("CustomBorderBrush", typeof(SolidColorBrush), typeof(CustomControlComboBoxThreeItems));
        public static readonly DependencyProperty CustomComboBoxStateProperty = DependencyProperty.Register("CustomComboBoxState", typeof(bool), typeof(CustomControlComboBoxThreeItems));
        public static readonly DependencyProperty Item1CommandProperty = DependencyProperty.Register("Item1Command", typeof(ICommand), typeof(CustomControlComboBoxThreeItems));
        public static readonly DependencyProperty Item1TextProperty = DependencyProperty.Register("Item1Text", typeof(string), typeof(CustomControlComboBoxThreeItems), new PropertyMetadata(""));
        public static readonly DependencyProperty Item2CommandProperty = DependencyProperty.Register("Item2Command", typeof(ICommand), typeof(CustomControlComboBoxThreeItems));
        public static readonly DependencyProperty Item2TextProperty = DependencyProperty.Register("Item2Text", typeof(string), typeof(CustomControlComboBoxThreeItems), new PropertyMetadata(""));
        public static readonly DependencyProperty Item3CommandProperty = DependencyProperty.Register("Item3Command", typeof(ICommand), typeof(CustomControlComboBoxThreeItems));
        public static readonly DependencyProperty Item3TextProperty = DependencyProperty.Register("Item3Text", typeof(string), typeof(CustomControlComboBoxThreeItems), new PropertyMetadata(""));

        #endregion Fields

        #region Constructors

        public CustomControlComboBoxThreeItems()
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

        public ICommand Item1Command
        {
            get => (ICommand)this.GetValue(Item1CommandProperty);
            set { this.SetValue(Item1CommandProperty, value); this.RaisePropertyChanged("Item1Command"); }
        }

        public String Item1Text
        {
            get => (String)this.GetValue(Item1TextProperty);
            set { this.SetValue(Item1TextProperty, value); this.RaisePropertyChanged("Item1Text"); }
        }

        public ICommand Item2Command
        {
            get => (ICommand)this.GetValue(Item2CommandProperty);
            set { this.SetValue(Item2CommandProperty, value); this.RaisePropertyChanged("Item2Command"); }
        }

        public String Item2Text
        {
            get => (String)this.GetValue(Item2TextProperty);
            set { this.SetValue(Item2TextProperty, value); this.RaisePropertyChanged("Item2Text"); }
        }

        public ICommand Item3Command
        {
            get => (ICommand)this.GetValue(Item3CommandProperty);
            set { this.SetValue(Item3CommandProperty, value); this.RaisePropertyChanged("Item3Command"); }
        }

        public String Item3Text
        {
            get => (String)this.GetValue(Item3TextProperty);
            set { this.SetValue(Item3TextProperty, value); this.RaisePropertyChanged("Item3Text"); }
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
