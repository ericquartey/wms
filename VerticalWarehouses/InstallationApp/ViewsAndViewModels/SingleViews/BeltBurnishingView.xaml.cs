using System.Diagnostics;
using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp
{
    /// <summary>
    /// Interaction logic for BeltBurnishingView.xaml
    /// </summary>
    public partial class BeltBurnishingView : UserControl
    {
        #region Constructors

        public BeltBurnishingView()
        {
            this.InitializeComponent();
            this.DataContext = ViewModels.BeltBurnishingVMInstance;
        }

        #endregion Constructors
    }
}
