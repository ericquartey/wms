using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommonServiceLocator;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for CardSensorShutter
    /// </summary>
    public partial class CardSensorShutter : UserControl
    {
        #region Fields

        public IMachineService machineService;

        private ISensorsService sensorsService;

        private IThemeService themeService;

        #endregion

        #region Constructors

        public CardSensorShutter()
        {
            this.InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.Loaded += (s, e) =>
            {
                this.OnAppeared();
            };
            this.Unloaded += (s, e) =>
            {
                this.Disappear();
            };
        }

        #endregion

        #region Methods

        protected void Disappear()
        {
            this.sensorsService = null;
        }

        protected void OnAppeared()
        {
            this.SubscribeToEvents();

            this.DataContext = this.sensorsService.ShutterSensors;

            this.Visibility = this.machineService.HasShutter ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        protected Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            return Task.CompletedTask;
        }

        private void SubscribeToEvents()
        {
            this.sensorsService = ServiceLocator.Current.GetInstance<ISensorsService>();
            this.machineService = ServiceLocator.Current.GetInstance<IMachineService>();
            this.themeService = ServiceLocator.Current.GetInstance<IThemeService>();
        }

        #endregion
    }
}
