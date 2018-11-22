using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.GatesHeightControl
{
    /// <summary>
    /// Interaction logic for GatesHeightControlNavigationButtonsView.xaml
    /// </summary>
    public partial class GatesHeightControlNavigationButtonsView : UserControl
    {
        #region Constructors

        public GatesHeightControlNavigationButtonsView()
        {
            this.InitializeComponent();
            this.DataContext = new GatesHeightControlNavigationButtonsViewModel();
            for (var i = 1; i <= DataManager.CurrentData.GeneralInfo.Bays_Quantity; i++)
            {
                var button = new Button();
                var style = this.FindResource("IAPP_NavigationViewButtonStyle") as Style;
                button.Style = style;
                button.Content = "Gate " + i + " Height Control";
                var b = new Binding("DataContext.Gate" + i + "HeightControlNavigationButtonCommand");
                b.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Window), 1);
                button.SetBinding(Button.CommandProperty, b);
                this.NavigationStackPanel.Children.Add(button);
            }
        }

        #endregion Constructors
    }
}
