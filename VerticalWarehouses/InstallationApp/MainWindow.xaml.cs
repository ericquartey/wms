using System.Windows;
using Ferretto.VW.Navigation;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System;
using System.Windows.Media;
using Ferretto.VW.CustomControls.Controls;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp
{
    public delegate void FinishedMachineModeChangeStateEvent();

    public delegate void FinishedMachineOnMarchChangeStateEvent();

    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();
            NavigationService.BackToVWAppEventHandler += () => this.Close();
            FinishedMachineModeChangeStateEventHandler += () => { };
            FinishedMachineOnMarchChangeStateEventHandler += () => { };
            MainWindowViewModel.ClickedOnMachineModeEventHandler += this.SetMachineMode;
            MainWindowViewModel.ClickedOnMachineOnMarchEventHandler += this.SetMachineOn;
            this.DataContext = new MainWindowViewModel();
        }

        #endregion Constructors

        #region Events

        public static event FinishedMachineModeChangeStateEvent FinishedMachineModeChangeStateEventHandler;

        public static event FinishedMachineOnMarchChangeStateEvent FinishedMachineOnMarchChangeStateEventHandler;

        #endregion Events

        #region Methods

        public void ChangeSkinToDark(object sender, MouseButtonEventArgs e)
        {
            NavigationService.RaiseChangeSkinToDarkEvent();
        }

        public void ChangeSkinToLight(object sender, MouseButtonEventArgs e)
        {
            NavigationService.RaiseChangeSkinToLightEvent();
        }

        public void ChangeSkinToMedium(object sender, MouseButtonEventArgs e)
        {
            NavigationService.RaiseChangeSkinToMediumEvent();
        }

        public void RaiseFinishedMachineModeChangeStateEvent() => FinishedMachineModeChangeStateEventHandler();

        public void RaiseFinishedMachineOnMarchChangeStateEvent() => FinishedMachineOnMarchChangeStateEventHandler();

        private async void SetMachineMode()
        {
            var ca = new ColorAnimation();
            ca.From = ((SolidColorBrush)Application.Current.Resources["VWAPP_MainWindowCustomComboBoxMachineMode_Manual"]).Color;
            ca.To = ((SolidColorBrush)Application.Current.Resources["VWAPP_MainWindowCustomComboBoxMachineMode_Auto"]).Color;
            ca.Duration = new Duration(TimeSpan.FromSeconds(.5));
            ca.RepeatBehavior = RepeatBehavior.Forever;
            this.MachineModeControl.RectangleBrush = new SolidColorBrush(this.MachineModeControl.RectangleBrush.Color);
            this.MachineModeControl.RectangleBrush.BeginAnimation(SolidColorBrush.ColorProperty, ca);
            await Task.Delay(3000);
            this.MachineModeControl.RectangleBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);
            this.MachineModeControl.RectangleBrush = (!this.MachineModeControl.MachineModeState) ? (SolidColorBrush)Application.Current.Resources["VWAPP_MainWindowCustomComboBoxMachineMode_Auto"] : (SolidColorBrush)Application.Current.Resources["VWAPP_MainWindowCustomComboBoxMachineMode_Manual"];
            this.RaiseFinishedMachineModeChangeStateEvent();
        }

        private async void SetMachineOn()
        {
            var ca = new ColorAnimation();
            ca.From = ((SolidColorBrush)Application.Current.Resources["VWAPP_MainWindowCustomComboBoxMachineOnMarch_Off"]).Color;
            ca.To = ((SolidColorBrush)Application.Current.Resources["VWAPP_MainWindowCustomComboBoxMachineOnMarch_On"]).Color;
            ca.Duration = new Duration(TimeSpan.FromSeconds(.5));
            ca.RepeatBehavior = RepeatBehavior.Forever;
            this.MachineOnMarchControl.RectangleBrush = new SolidColorBrush(this.MachineOnMarchControl.RectangleBrush.Color);
            this.MachineOnMarchControl.RectangleBrush.BeginAnimation(SolidColorBrush.ColorProperty, ca);
            await Task.Delay(3000);
            this.MachineOnMarchControl.RectangleBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);
            this.MachineOnMarchControl.RectangleBrush = (!this.MachineOnMarchControl.MachineOnMarchState) ? (SolidColorBrush)Application.Current.Resources["VWAPP_MainWindowCustomComboBoxMachineOnMarch_On"] : (SolidColorBrush)Application.Current.Resources["VWAPP_MainWindowCustomComboBoxMachineOnMarch_Off"];
            this.RaiseFinishedMachineOnMarchChangeStateEvent();
        }

        #endregion Methods
    }
}
