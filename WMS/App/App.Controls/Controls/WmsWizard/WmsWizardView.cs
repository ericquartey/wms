using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using DevExpress.Mvvm.UI;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;

namespace Ferretto.WMS.App.Controls
{
    public class WmsWizardView : WmsDialogView
    {
        #region Fields

        public static readonly DependencyProperty StartModuleNameProperty = DependencyProperty.Register(
                                                                            nameof(StartModuleName),
                                                                            typeof(string),
                                                                            typeof(WmsWizardView),
                                                                            new FrameworkPropertyMetadata(default(string), null));

        public static readonly DependencyProperty StartViewNameProperty = DependencyProperty.Register(
                                                                    nameof(StartViewName),
                                                                    typeof(string),
                                                                    typeof(WmsWizardView),
                                                                    new FrameworkPropertyMetadata(default(string), null));

        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();

        private readonly INavigationService navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

        private readonly Stack<INavigableView> registeredViews = new Stack<INavigableView>();

        private readonly object stepsEventSubscription;

        private INavigableView currentView;

        private ContentControl currentViewContainer;

        #endregion

        #region Constructors

        public WmsWizardView()
        {
            this.stepsEventSubscription = this.eventService
                   .Subscribe<StepsPubSubEvent>(this.CommandExecuteEvent);
        }

        #endregion

        #region Properties

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

        public override void OnWMSDialogView_Loaded(object sender, RoutedEventArgs e)
        {
            base.OnWMSDialogView_Loaded(sender, e);

            if (string.IsNullOrEmpty(this.StartModuleName))
            {
                throw new ArgumentException(nameof(this.StartModuleName));
            }

            if (string.IsNullOrEmpty(this.StartViewName))
            {
                throw new ArgumentException(nameof(this.StartViewName));
            }

            this.AssociateView();
        }

        internal virtual void GoToNext()
        {
            this.registeredViews.Push(this.currentView);
            var(moduleName, viewName, data) = this.GetStepViewModel().GetNextView();
            this.AssociateNewView(moduleName, viewName, data);
        }

        internal virtual void GoToPrevious()
        {
            this.currentView.Disappear();
            this.currentView = this.registeredViews.Pop();
            this.currentViewContainer.Content = this.currentView;
            this.Refresh();
        }

        internal virtual void Save()
        {
            this.GetStepViewModel().Save();
        }

        protected override void OnClosed(EventArgs e)
        {
            this.eventService.Unsubscribe<StepsPubSubEvent>(this.stepsEventSubscription);

            this.currentView.Disappear();

            foreach (var view in this.registeredViews)
            {
                view.Disappear();
            }

            this.registeredViews.Clear();

            base.OnClosed(e);
        }

        private void AssociateNewView(string moduleName, string viewName, object data)
        {
            this.currentView = this.navigationService.GetNewView(moduleName, viewName, data);
            this.currentViewContainer.Content = this.currentView;
        }

        private void AssociateView()
        {
            var container = LayoutTreeHelper.GetVisualChildren(this).OfType<ContentControl>().FirstOrDefault(c => c.Tag != null && c.Tag.Equals(General.WizardStep));
            if (container is ContentControl contentControl)
            {
                this.currentViewContainer = contentControl;
                this.AssociateNewView(this.StartModuleName, this.StartViewName, this.Data);
            }
        }

        private void Cancel()
        {
            this.Disappear();
        }

        private bool CanGoToNext()
        {
            return this.GetStepViewModel().CanGoToNextView();
        }

        private bool CanGoToPrevious()
        {
            return this.registeredViews.Count > 0;
        }

        private void CommandExecuteEvent(StepsPubSubEvent e)
        {
            if (this.currentView == null ||
                !(((FrameworkElement)this.currentViewContainer.Content).DataContext is IStepNavigableViewModel))
            {
                e.CanExecute = false;
                return;
            }

            switch (e.CommandExecute)
            {
                case CommandExecuteType.CanPrevious:
                    e.CanExecute = this.CanGoToPrevious();
                    break;

                case CommandExecuteType.CanNext:
                    e.CanExecute = this.CanGoToNext();
                    break;

                case CommandExecuteType.Previous:
                    this.GoToPrevious();
                    break;

                case CommandExecuteType.Next:
                    this.GoToNext();
                    break;

                case CommandExecuteType.Save:
                    this.Save();
                    break;

                case CommandExecuteType.Cancel:
                    this.Cancel();
                    break;

                case CommandExecuteType.Refresh:
                    this.Refresh();
                    break;
            }
        }

        private IStepNavigableViewModel GetStepViewModel()
        {
            return ((FrameworkElement)this.currentViewContainer.Content).DataContext as IStepNavigableViewModel;
        }

        private void Refresh()
        {
            ((IStepsViewModel)this.DataContext).Refresh();
        }

        #endregion
    }
}
