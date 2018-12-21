using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp
{
    /// <summary>
    /// Interaction logic for SSBaysView.xaml
    /// </summary>
    public partial class SSBaysView : UserControl
    {
        #region Constructors

        public SSBaysView()
        {
            this.InitializeComponent();
            this.DataContext = ViewModels.SSBaysVMInstance;
        }

        #endregion Constructors
    }
}
