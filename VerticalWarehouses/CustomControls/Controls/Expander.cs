using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Ferretto.VW.CustomControls.Controls
{
    public class Expander : System.Windows.Controls.Expander
    {
        #region Fields

        public static readonly DependencyProperty HideToggleButtonProperty = DependencyProperty.RegisterAttached("HideToggleButton", typeof(bool), typeof(Expander), new FrameworkPropertyMetadata(false));
        private Grid gridHeaderSite;
        private ToggleButton toggleButton;
        private VisualTreeAdapter visualTreeAdapterInstance;

        #endregion Fields

        #region Constructors

        public Expander()
        {
            this.Style = Application.Current.Resources["WmsExpanderStyle"] as Style;
            this.visualTreeAdapterInstance = new VisualTreeAdapter(this);
        }

        #endregion Constructors

        #region Properties

        public bool HideToggleButton
        {
            get => (bool)this.GetValue(HideToggleButtonProperty);
            set => this.SetValue(HideToggleButtonProperty, value);
        }

        #endregion Properties

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ((FrameworkElement)this.Parent).SizeChanged += this.Expander_SizeChanged;
        }

        private void Expander_SizeChanged(Object sender, SizeChangedEventArgs e)
        {
            if (this.visualTreeAdapterInstance == null)
            {
                this.visualTreeAdapterInstance = new VisualTreeAdapter(this);
            }
            if (!(sender is FrameworkElement parentControl))
            {
                return;
            }
            if (this.toggleButton == null)
            {
                var toggleButtonFound = this.visualTreeAdapterInstance.Children()
                 .OfType<ToggleButton>()
                 .FirstOrDefault();
                if (toggleButtonFound != null)
                {
                    this.toggleButton = toggleButtonFound as ToggleButton;
                }
            }
            if (this.gridHeaderSite == null)
            {
                var gridFound = this.visualTreeAdapterInstance.Children()
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
