using System.Windows.Controls;
using System.Windows.Input;

namespace Ferretto.VW.InstallationApp
{
    public partial class LSMTVerticalEngineView : UserControl
    {
        #region Constructors

        public LSMTVerticalEngineView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private async void MoveDownVerticalAxisHandlerAsync(object sender, MouseButtonEventArgs e)
        {
            await (this.DataContext as LSMTVerticalEngineViewModel)?.MoveDownVerticalAxisAsync();
        }

        private async void MoveUpVerticalAxisHandlerAsync(object sender, MouseButtonEventArgs e)
        {
            await (this.DataContext as LSMTVerticalEngineViewModel)?.MoveUpVerticalAxisAsync();
        }

        private async void StopVerticalAxisHandlerAsync(object sender, MouseButtonEventArgs e)
        {
            await (this.DataContext as LSMTVerticalEngineViewModel)?.StopVerticalAxisAsync();
        }

        #endregion
    }
}
