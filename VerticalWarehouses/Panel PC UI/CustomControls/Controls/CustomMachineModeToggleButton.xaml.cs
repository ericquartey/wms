using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for CustomMachineModeToggleButton.xaml
    /// </summary>
    public partial class CustomMachineModeToggleButton : PpcControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty CustomCommandProperty = DependencyProperty.Register("CustomCommand", typeof(ICommand), typeof(CustomMachineModeToggleButton));

        public static readonly DependencyProperty MachineModeStateProperty = DependencyProperty.Register("MachineModeState", typeof(bool), typeof(CustomMachineModeToggleButton));

        public static readonly DependencyProperty MachineModeStringProperty = DependencyProperty.Register("MachineModeString", typeof(string), typeof(CustomMachineModeToggleButton));

        public static readonly DependencyProperty RectangleBrushProperty = DependencyProperty.Register("RectangleBrush", typeof(SolidColorBrush), typeof(CustomMachineModeToggleButton));

        #endregion

        #region Constructors

        public CustomMachineModeToggleButton()
        {
            this.InitializeComponent();
            this.PresentationType = PresentationTypes.MachineMode;
            var customMachineModeToggleButton = this;
            this.LayoutRoot.DataContext = customMachineModeToggleButton;
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

        public bool MachineModeState
        {
            get => (bool)this.GetValue(MachineModeStateProperty);
            set
            {
                this.SetValue(MachineModeStateProperty, value);
                this.RaisePropertyChanged(nameof(this.MachineModeState));
            }
        }

        public string MachineModeString
        {
            get => (string)this.GetValue(MachineModeStringProperty);
            set
            {
                this.SetValue(MachineModeStringProperty, value);
                this.RaisePropertyChanged(nameof(this.MachineModeString));
            }
        }

        public SolidColorBrush RectangleBrush
        {
            get => (SolidColorBrush)this.GetValue(RectangleBrushProperty);
            set
            {
                this.SetValue(RectangleBrushProperty, value);
                this.RaisePropertyChanged(nameof(this.RectangleBrush));
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
