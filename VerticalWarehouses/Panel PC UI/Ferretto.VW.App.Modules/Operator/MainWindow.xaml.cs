using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Modules.Operator.Resources;
using Ferretto.VW.App.Modules.Operator.Resources.Enumerations;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator
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
            this.InitializeComponent();

            this.eventAggregator = eventAggregator;
            this.eventAggregator.GetEvent<OperatorApp_Event>().Subscribe(
                (message) => { this.HideAndUnsubscribe(); },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == OperatorApp_EventMessageType.BackToVWApp);

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

            this.RaiseFinishedMachineModeChangeStateEvent();
        }

        private async void SetMachineOn()
        {
            //var ca = new ColorAnimation();
            //ca.From = ((SolidColorBrush)Application.Current.Resources["VWAPP_MainWindowCustomComboBoxMachineOnMarch_Off"]).Color;
            //ca.To = ((SolidColorBrush)Application.Current.Resources["VWAPP_MainWindowCustomComboBoxMachineOnMarch_On"]).Color;
            //ca.Duration = new Duration(TimeSpan.FromSeconds(.5));
            //ca.RepeatBehavior = RepeatBehavior.Forever;
            //this.MachineOnMarchControl.RectangleBrush = new SolidColorBrush(this.MachineOnMarchControl.RectangleBrush.Color);
            //this.MachineOnMarchControl.RectangleBrush.BeginAnimation(SolidColorBrush.ColorProperty, ca);
            //await Task.Delay(3000);
            //this.MachineOnMarchControl.RectangleBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);
            //this.MachineOnMarchControl.RectangleBrush = (!this.MachineOnMarchControl.MachineOnMarchState) ?
            //    (SolidColorBrush)Application.Current.Resources["VWAPP_MainWindowCustomComboBoxMachineOnMarch_On"] :
            //    (SolidColorBrush)Application.Current.Resources["VWAPP_MainWindowCustomComboBoxMachineOnMarch_Off"];
            this.RaiseFinishedMachineOnMarchChangeStateEvent();
        }

        private void UnsubscribeEvents()
        {
            FinishedMachineModeChangeStateEventHandler = null;
            FinishedMachineOnMarchChangeStateEventHandler = null;
        }

        #endregion
    }
}
