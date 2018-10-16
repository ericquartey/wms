using System.Windows.Controls;

namespace Ferretto.VW.CustomControls.Controls
{
    public partial class CustomComboBox : UserControl
    {
        #region Constructors

        public CustomComboBox()
        {
            this.DataContext = new CustomComboBoxViewModel();
            this.InitializeComponent();
        }

        #endregion Constructors
    }
}
