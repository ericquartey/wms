using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm.UI;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Utils;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls
{
    public partial class WmsHistoryView : ContentControl, IWmsHistoryView
    {
        #region Fields

        public static readonly DependencyProperty StartModuleNameProperty = DependencyProperty.Register("StartModuleName", typeof(string), typeof(WmsHistoryView));
        public static readonly DependencyProperty StartViewNameProperty = DependencyProperty.Register("StartViewName", typeof(string), typeof(WmsHistoryView));
        private readonly INavigationService navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
        private readonly Stack<INavigableView> registeredViews = new Stack<INavigableView>();
        private ActionBar actionBarHsitoryView;

        #endregion Fields

        #region Constructors

        public WmsHistoryView()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

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

        #endregion Properties

        #region Methods

        public void Appear(string moduleName, string viewModelName, object data = null)
        {
            if (string.IsNullOrEmpty(viewModelName))
            {
                return;
            }

            var modelName = MvvmNaming.GetModelNameFromViewModelName(viewModelName);
            var moduleViewName = MvvmNaming.GetViewName(moduleName, modelName);
            var id = this.navigationService.GetViewModelBindFirstId(moduleViewName);
            if (id == null)
            {
                return;
            }
            var instanceModuleViewName = $"{moduleViewName}.{id}";
            var registeredView = this.navigationService.GetView(instanceModuleViewName, data);
            this.registeredViews.Push(registeredView);
            this.Content = registeredView;
            this.CheckBackVisibility();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.actionBarHsitoryView = LayoutTreeHelper.GetVisualChildren(this as DependencyObject)
                .OfType<ActionBar>()
                .FirstOrDefault();

            if (string.IsNullOrEmpty(this.StartModuleName) == false &&
                string.IsNullOrEmpty(this.StartViewName) == false)
            {
                var data = this.GetParentWmsViewData();
                this.Appear(this.StartModuleName, this.StartViewName, data);
            }
        }

        public void Previous()
        {
            if (this.registeredViews.Count() == 1)
            {
                return;
            }

            this.registeredViews.Pop();
            this.Content = this.registeredViews.Last();
            this.CheckBackVisibility();
        }

        private void BackViewClick(Object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            this.Previous();
        }

        private void CheckBackVisibility()
        {
            this.actionBarHsitoryView.Visibility = (this.registeredViews.Count() == 1) ? Visibility.Hidden : Visibility.Visible;
        }

        private Object GetParentWmsViewData()
        {
            var parentWmsView = LayoutTreeHelper.GetVisualParents(this as DependencyObject)
               .OfType<WmsView>()
               .FirstOrDefault();
            if (parentWmsView != null)
            {
                return parentWmsView.Data;
            }
            return null;
        }

        #endregion Methods
    }
}
