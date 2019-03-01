using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System;
using System.Windows.Media;
using Ferretto.VW.CustomControls.Controls;
using System.Threading.Tasks;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.InstallationApp.Resources.Enumerables;
using Prism.Events;

namespace Ferretto.VW.InstallationApp
{
    public delegate void FinishedMachineModeChangeStateEvent();

    public delegate void FinishedMachineOnMarchChangeStateEvent();

    public partial class MainWindow : Window, IMainWindow
    {
        #region Fields

        private IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public MainWindow(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.InitializeComponent();
            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                (message) => { this.HideAndUnsubscribe(); }, ThreadOption.PublisherThread, false,
                message => message.Type == InstallationApp_EventMessageType.BackToVWApp);
            FinishedMachineModeChangeStateEventHandler += () => { };
            FinishedMachineOnMarchChangeStateEventHandler += () => { };
            MainWindowViewModel.ClickedOnMachineModeEventHandler += this.SetMachineMode;
            MainWindowViewModel.ClickedOnMachineOnMarchEventHandler += this.SetMachineOn;
        }

        #endregion

        #region Events

        public static event FinishedMachineModeChangeStateEvent FinishedMachineModeChangeStateEventHandler;

        public static event FinishedMachineOnMarchChangeStateEvent FinishedMachineOnMarchChangeStateEventHandler;

        #endregion

        #region Methods

        public void RaiseFinishedMachineModeChangeStateEvent() => FinishedMachineModeChangeStateEventHandler();

        public void RaiseFinishedMachineOnMarchChangeStateEvent() => FinishedMachineOnMarchChangeStateEventHandler();

        protected override void OnClosed(EventArgs e)
        {
            this.UnsubscribeEvents();
            base.OnClosed(e);
        }

        private void Button_Click(Object sender, RoutedEventArgs e)
        {
        }

        private void HideAndUnsubscribe()
        {
            this.UnsubscribeEvents();
            this.Hide();
        }

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

        private void UnsubscribeEvents()
        {
            MainWindow.FinishedMachineModeChangeStateEventHandler = null;
            MainWindow.FinishedMachineOnMarchChangeStateEventHandler = null;
        }

        #endregion
    }
}
