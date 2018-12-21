using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp
{
    /// <summary>
    /// Interaction logic for SSVariousInputsView.xaml
    /// </summary>
    public partial class SSVariousInputsView : UserControl
    {
        #region Constructors

        public SSVariousInputsView()
        {
            this.InitializeComponent();
            this.DataContext = ViewModels.SSVariousInputsVMInstance;
        }

        #endregion Constructors
    }
}
