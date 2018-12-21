using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp
{
    /// <summary>
    /// Interaction logic for Gate3HeightControlView.xaml
    /// </summary>
    public partial class Gate3HeightControlView : UserControl
    {
        #region Constructors

        public Gate3HeightControlView()
        {
            this.InitializeComponent();
            this.DataContext = ViewModels.Gate3HeightControlVMInstance;
        }

        #endregion Constructors
    }
}
