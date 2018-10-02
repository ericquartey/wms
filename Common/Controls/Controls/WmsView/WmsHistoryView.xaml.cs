using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Ferretto.Common.Controls.Interfaces;

namespace Ferretto.Common.Controls
{
    public partial class WmsHistoryView : UserControl, IWmsHistoryView
    {
        #region Fields

        public static readonly DependencyProperty ViewContentProperty = DependencyProperty.Register("ViewContent", typeof(object), typeof(WmsHistoryView));
        public static readonly DependencyProperty ViewContentTemplateProperty = DependencyProperty.Register("ViewContentTemplate", typeof(DataTemplate), typeof(WmsHistoryView));
        public static readonly DependencyProperty ViewContentTemplateSelectorProperty = DependencyProperty.Register("ViewContentTemplateSelector", typeof(DataTemplateSelector), typeof(WmsHistoryView));
        private readonly Dictionary<string, INavigableView> registeredViews = new Dictionary<string, INavigableView>();

        #endregion Fields

        // BackCommand

        #region Constructors

        public WmsHistoryView()
        {
            this.DataContext = this;
        }

        #endregion Constructors

        #region Properties

        public object ViewContent
        {
#pragma warning disable IDE0009 // Member access should be qualified.
            get => GetValue(ViewContentProperty);
            set => this.SetValue(ViewContentProperty, value);
#pragma warning restore IDE0009 // Member access should be qualified.
        }

        #endregion Properties

        #region Methods

        public void Appear(string viewModelName)
        {
            if (string.IsNullOrEmpty(viewModelName))
            {
                return;
            }

            if (this.registeredViews.ContainsKey(viewModelName))
            {
                this.ViewContent = this.registeredViews[viewModelName];
            }
            //if (this.existView(view) == false)
            //{
            //    this.AddView(view);
            //}
            // this.ViewContent = new WmsHistoryViewModel();
        }

        private void AddView(string viewModelName, INavigableView view)
        {
            this.registeredViews.Add(viewModelName, view);
        }

        private Boolean existView(INavigableView view)
        {
            return (this.registeredViews.FirstOrDefault(v => v.Value.MapId == view.MapId).Value != null);
        }

        #endregion Methods
    }
}
