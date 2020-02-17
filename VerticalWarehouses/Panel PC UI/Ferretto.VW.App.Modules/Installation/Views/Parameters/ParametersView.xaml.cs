using Ferretto.VW.App.Controls;

namespace Ferretto.VW.App.Modules.Installation.Views
{
    public partial class ParametersView : View
    {
        #region Constructors

        public ParametersView()
        {
            this.InitializeComponent();
            this.Loaded += this.ParametersView_Loaded;
            this.Unloaded += this.ParametersView_Unloaded;
        }

        #endregion

        #region Methods

        private void ParametersView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // this is heavy (cannot dispose handler)
            // this.Loaded -= this.ParametersView_Loaded;
            try
            {
                if (this.DataContext is BaseNavigationViewModel viewModel)
                {
                    viewModel.NavigatingBack += this.ViewModel_NavigatingBack;
                }

                this.scaffolder?.ResetNavigation();
            }
            catch
            {
            }
        }

        private void ParametersView_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is BaseNavigationViewModel viewModel)
            {
                viewModel.NavigatingBack -= this.ViewModel_NavigatingBack;
            }
        }

        private void ViewModel_NavigatingBack(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (e.Cancel = this.scaffolder.IsNavigating)
            {
                this.scaffolder.Back();
            }
        }

        #endregion
    }
}
