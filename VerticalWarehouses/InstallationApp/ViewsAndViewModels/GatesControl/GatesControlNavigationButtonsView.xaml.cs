using System.Windows.Controls;
using Ferretto.VW.Utils.Source;
using Ferretto.VW.CustomControls.Styles;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.GatesControl
{
    /// <summary>
    /// Interaction logic for GatesControlNavigationButtonsView.xaml
    /// </summary>
    public partial class GatesControlNavigationButtonsView : UserControl
    {
        #region Constructors

        public GatesControlNavigationButtonsView()
        {
            this.InitializeComponent();
            this.DataContext = new GatesControlNavigationButtonsViewModel();
            for (var i = 1; i <= DataManager.CurrentData.GeneralInfo.Bays_Quantity; i++)
            {
                var button = new Button();
                var style = this.FindResource("IAPP_NavigationViewButtonStyle") as Style;
                button.Style = style;
                button.Content = "Gate " + i + " Control";
                var b = new Binding("DataContext.Gates" + i + "ControlNavigationButtonCommand");
                b.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Window), 1);
                button.SetBinding(Button.CommandProperty, b);
                this.NavigationStackPanel.Children.Add(button);
            }
        }

        #endregion Constructors
    }
}
