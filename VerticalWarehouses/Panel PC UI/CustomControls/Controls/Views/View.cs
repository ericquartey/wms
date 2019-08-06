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
            this.Loaded += this.View_Loaded;
        }

        #endregion

        #region Methods

        private void View_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is INavigableViewModel viewModel)
            {
                viewModel.OnNavigated();
            }
        }

        #endregion
    }
}
