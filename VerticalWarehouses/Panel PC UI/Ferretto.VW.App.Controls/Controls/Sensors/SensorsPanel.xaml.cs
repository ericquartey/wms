using System.Windows.Controls;
using CommonServiceLocator;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls
{
    /// <summary>
    /// Interaction logic for SensorsPanel.xaml
    /// </summary>
    public partial class SensorsPanel : UserControl
    {
        private ISensorsService sensorsService;

        #region Constructors

        public SensorsPanel()
        {
            this.InitializeComponent();

            this.sensorsService = ServiceLocator.Current.GetInstance<ISensorsService>();

            this.DataContext = this.sensorsService;

            this.Loaded += this.SensorsPanel_Loaded;
            this.Unloaded += this.SensorsPanel_Unloaded;
        }

        private void SensorsPanel_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.sensorsService.EndMonitoring();
        }

        #endregion

        #region Methods

        private void SensorsPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.sensorsService.StartMonitoring();
        }

        #endregion
    }
}
