using System.Threading.Tasks;
using System.Windows.Controls;
using Ferretto.VW.App.Services.Interfaces;

namespace Ferretto.VW.App.Controls
{
    public interface IView
    {
    }

    public class View : UserControl, IView
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
            if (this.DataContext is INavigableViewModel viewModel)
            {
                await viewModel.OnNavigatedAsync();
            }
        }

        #endregion
    }
}
