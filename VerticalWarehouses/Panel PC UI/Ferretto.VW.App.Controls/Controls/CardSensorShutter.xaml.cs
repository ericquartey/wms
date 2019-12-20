using System.ComponentModel;
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

        private readonly ISensorsService sensorsService;

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
                this.sensorsService.RefreshAsync(true);
            };

            this.sensorsService = ServiceLocator.Current.GetInstance<ISensorsService>();

            this.DataContext = this.sensorsService.ShutterSensors;
        }

        #endregion
    }
}
