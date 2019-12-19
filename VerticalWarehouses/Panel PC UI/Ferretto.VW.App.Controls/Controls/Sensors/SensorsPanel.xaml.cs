using System.ComponentModel;
using System.Windows.Controls;
using CommonServiceLocator;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls
{
    public partial class SensorsPanel : UserControl
    {
        #region Fields

        private readonly IMachineService machineService;

        private readonly ISensorsService sensorsService;

        #endregion

        #region Constructors

        public SensorsPanel()
        {
            this.InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.Loaded += this.SensorsPanel_Loaded;

            this.machineService = ServiceLocator.Current.GetInstance<IMachineService>();
            this.sensorsService = ServiceLocator.Current.GetInstance<ISensorsService>();

            this.DataContext = new
            {
                MachineService = this.machineService,
                MachineStatus = this.machineService.MachineStatus,
                SensorsService = this.sensorsService
            };
        }

        #endregion

        #region Methods

        private void SensorsPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.sensorsService.RefreshAsync(true);
        }

        #endregion
    }
}
