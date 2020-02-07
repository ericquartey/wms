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
        }

        #endregion

        #region Methods

        private void ParametersView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // this is heavy (cannot dispose handler)
            // this.Loaded -= this.ParametersView_Loaded;
            try
            {
                this.scaffolder?.ResetNavigation();
            }
            catch
            {
            }
        }

        #endregion
    }
}
