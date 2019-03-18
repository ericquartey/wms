using System;
using System.Windows;
using System.Windows.Interactivity;
using Ferretto.Common.Controls.Interfaces;
using CommonServiceLocator;

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

        #endregion

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

        #endregion

        #region Methods

        protected override void Invoke(object parameter)
        {
            if (string.IsNullOrEmpty(this.StartModuleName) == false &&
                string.IsNullOrEmpty(this.StartViewName) == false)
            {
                this.historyViewService.Appear(this.StartModuleName, this.StartViewName, this.Id);
                if (parameter is RoutedEventArgs routedArgs)
                {
                    routedArgs.Handled = this.IsHandled;
                }
            }
            else
            {
                throw new ArgumentException(Ferretto.Common.Resources.Errors.WmsHistoryViewAppearSyntax);
            }
        }

        #endregion
    }
}
