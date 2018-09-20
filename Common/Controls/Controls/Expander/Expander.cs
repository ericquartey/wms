using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DevExpress.Mvvm.UI;

namespace Ferretto.Common.Controls
{
    public class Expander : System.Windows.Controls.Expander
    {
        #region Fields

        private Grid gridHeaderSite;
        private ToggleButton toggleButton;

        #endregion Fields

        #region Constructors

        public Expander()
        {
            this.Style = Application.Current.Resources["WmsExpanderStyle"] as Style;
        }

        #endregion Constructors

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ((FrameworkElement)this.Parent).SizeChanged += this.Expander_SizeChanged;
        }

        private void Expander_SizeChanged(Object sender, SizeChangedEventArgs e)
        {
            if (!(sender is FrameworkElement parentControl))
            {
                return;
            }
            if (this.toggleButton == null)
            {
                var toggleButtonFound = LayoutTreeHelper.GetVisualChildren(this as DependencyObject)
                 .OfType<ToggleButton>()
                 .FirstOrDefault();
                if (toggleButtonFound != null)
                {
                    this.toggleButton = toggleButtonFound as ToggleButton;
                }
            }
            if (this.gridHeaderSite == null)
            {
                var gridFound = LayoutTreeHelper.GetVisualChildren(this as DependencyObject)
                 .OfType<Grid>()
                 .FirstOrDefault(n => n.Name == "GridHeaderSite");
                if (gridFound != null)
                {
                    this.gridHeaderSite = gridFound as Grid;
                }
            }
            var actualHeight = (this.ActualHeight - 4);
            if (this.gridHeaderSite.Height != actualHeight)
            {
                this.gridHeaderSite.Height = actualHeight;
            }
        }

        #endregion Methods
    }
}
