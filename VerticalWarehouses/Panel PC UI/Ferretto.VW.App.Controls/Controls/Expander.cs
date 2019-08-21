using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Ferretto.VW.App.Controls.Utils;

namespace Ferretto.VW.App.Controls.Controls
{
    public class Expander : System.Windows.Controls.Expander
    {
        #region Fields

        public static readonly DependencyProperty HideToggleButtonProperty = DependencyProperty.RegisterAttached(
            nameof(HideToggleButton),
            typeof(bool),
            typeof(Expander),
            new FrameworkPropertyMetadata(false));

        private Grid gridHeaderSite;

        private ToggleButton toggleButton;

        private VisualTreeAdapter visualTreeAdapterInstance;

        #endregion

        #region Constructors

        public Expander()
        {
            this.Style = Application.Current.Resources["WmsExpanderStyle"] as Style;
            this.visualTreeAdapterInstance = new VisualTreeAdapter(this);
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
            if (this.visualTreeAdapterInstance == null)
            {
                this.visualTreeAdapterInstance = new VisualTreeAdapter(this);
            }
            if (sender is FrameworkElement parentControl)
            {
                if (this.toggleButton == null)
                {
                    var toggleButtonFound = this.visualTreeAdapterInstance.Children()
                     .OfType<ToggleButton>()
                     .FirstOrDefault();
                    if (toggleButtonFound != null)
                    {
                        this.toggleButton = toggleButtonFound;
                    }
                }
                if (this.gridHeaderSite == null)
                {
                    var gridFound = this.visualTreeAdapterInstance.Children()
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
        }

        #endregion
    }
}
