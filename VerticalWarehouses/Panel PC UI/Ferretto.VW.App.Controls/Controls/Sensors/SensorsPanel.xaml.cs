using System.Windows.Controls;
using CommonServiceLocator;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls
{
    public partial class SensorsPanel : UserControl
    {
        #region Fields

        private readonly ISensorsService sensorsService;

        #endregion

        #region Constructors

        public SensorsPanel()
        {
            this.InitializeComponent();

            this.Loaded += this.SensorsPanel_Loaded;

            this.sensorsService = ServiceLocator.Current.GetInstance<ISensorsService>();
            this.DataContext = this.sensorsService;
        }

        #endregion

        #region Methods

        private void SensorsPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.sensorsService.RefreshAsync();
        }

        #endregion
    }
}
