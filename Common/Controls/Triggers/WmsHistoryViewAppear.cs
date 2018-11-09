using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls
{
    public class WmsHistoryViewAppear : TriggerAction<FrameworkElement>
    {
        #region Fields

        public static readonly DependencyProperty IdProperty = DependencyProperty.Register(nameof(Id), typeof(object), typeof(WmsHistoryViewAppear));
        public static readonly DependencyProperty IsHandledProperty = DependencyProperty.Register(nameof(IsHandled), typeof(bool), typeof(WmsHistoryViewAppear), new PropertyMetadata(true));
        public static readonly DependencyProperty StartModuleNameProperty = DependencyProperty.Register(nameof(StartModuleName), typeof(string), typeof(WmsHistoryViewAppear));
        public static readonly DependencyProperty StartViewNameProperty = DependencyProperty.Register(nameof(StartViewName), typeof(string), typeof(WmsHistoryViewAppear));
        private readonly IHistoryViewService historyViewService = ServiceLocator.Current.GetInstance<IHistoryViewService>();
        private readonly IInputService inputService = ServiceLocator.Current.GetInstance<IInputService>();
        private readonly INavigationService navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
        private bool isControlPressed;

        #endregion Fields

        #region Constructors

        public WmsHistoryViewAppear()
        {
            this.inputService.BeginMouseNotify(this, this.OnMouseDown);
        }

        #endregion Constructors

        #region Properties

        public object Id
        {
            get => this.GetValue(IdProperty);
            set => this.SetValue(IdProperty, value);
        }

        public bool IsHandled
        {
            get => (bool)this.GetValue(IsHandledProperty);
            set => this.SetValue(IsHandledProperty, value);
        }

        public string StartModuleName
        {
            get => (string)this.GetValue(StartModuleNameProperty);
            set => this.SetValue(StartModuleNameProperty, value);
        }

        public string StartViewName
        {
            get => (string)this.GetValue(StartViewNameProperty);
            set => this.SetValue(StartViewNameProperty, value);
        }

        #endregion Properties

        #region Methods

        protected override void Invoke(object args)
        {
            if (string.IsNullOrEmpty(this.StartModuleName) == false &&
                string.IsNullOrEmpty(this.StartViewName) == false)
            {
                if (this.isControlPressed)
                {
                    this.navigationService.Appear(this.StartModuleName, this.StartViewName, this.Id);
                }
                else
                {
                    this.historyViewService.Appear(this.StartModuleName, this.StartViewName, this.Id);
                }
                this.isControlPressed = false;
                if (args is RoutedEventArgs routedArgs)
                {
                    routedArgs.Handled = this.IsHandled;
                }
            }
            else
            {
                throw new ArgumentException(Ferretto.Common.Resources.Errors.WmsHistoryViewAppearSyntax);
            }
        }

        private void OnMouseDown(MouseDownInfo mouseDownInfo)
        {
            this.isControlPressed = ((Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control);
        }

        #endregion Methods
    }
}
