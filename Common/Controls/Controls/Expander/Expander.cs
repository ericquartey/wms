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

        public static readonly DependencyProperty HideToggleButtonProperty = DependencyProperty.RegisterAttached(
            "HideToggleButton",
            typeof(bool),
            typeof(Expander),
            new FrameworkPropertyMetadata(false));

        private Grid gridHeaderSite;
        private ToggleButton toggleButton;

        #endregion

        #region Constructors

        public Expander()
        {
            this.Style = Application.Current.Resources["WmsExpanderStyle"] as Style;
        }

        #endregion

        #region Properties

        public bool HideToggleButton
        {
            get => (bool)this.GetValue(HideToggleButtonProperty);
            set => this.SetValue(HideToggleButtonProperty, value);
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ((FrameworkElement)this.Parent).SizeChanged += this.Expander_SizeChanged;
        }

        private void Expander_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!(sender is FrameworkElement))
            {
                return;
            }

            if (this.toggleButton == null)
            {
                var toggleButtonFound = LayoutTreeHelper.GetVisualChildren(this)
                 .OfType<ToggleButton>()
                 .FirstOrDefault();
                if (toggleButtonFound != null)
                {
                    this.toggleButton = toggleButtonFound;
                }
            }

            if (this.gridHeaderSite == null)
            {
                var gridFound = LayoutTreeHelper.GetVisualChildren(this)
                 .OfType<Grid>()
                 .FirstOrDefault(n => n.Name == "GridHeaderSite");
                if (gridFound != null)
                {
                    this.gridHeaderSite = gridFound;
                }
            }

            var actualHeight = this.ActualHeight - 4;
            this.gridHeaderSite.Height = actualHeight;
        }

        #endregion
    }
}
