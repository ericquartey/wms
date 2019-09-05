using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using Ferretto.VW.App.Services.Interfaces;

namespace Ferretto.VW.App.Controls
{
    public class View : UserControl
    {
        #region Constructors

        public View()
        {
            this.Loaded += async (sender, e) => await this.View_Loaded(sender, e);
        }

        #endregion

        #region Methods

        private async Task View_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is IActivationViewModel activationViewModel)
            {
                this.SetBinding(
                    IsEnabledProperty,
                    new Binding(nameof(IActivationViewModel.IsEnabled)) { Source = activationViewModel });
            }

            if (this.DataContext is INavigableViewModel viewModel)
            {
                await viewModel.OnNavigatedAsync();
            }
        }

        #endregion
    }
}
