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
using Ferretto.VW.InstallationApp.ViewModels;

namespace Ferretto.VW.InstallationApp.Views
{
    /// <summary>
    /// Interaction logic for SSVerticalAxisView.xaml
    /// </summary>
    public partial class SSVerticalAxisView : UserControl
    {
        #region Constructors

        public SSVerticalAxisView()
        {
            this.InitializeComponent();
            this.DataContext = new SSVerticalAxisViewModel();
        }

        #endregion Constructors
    }
}
