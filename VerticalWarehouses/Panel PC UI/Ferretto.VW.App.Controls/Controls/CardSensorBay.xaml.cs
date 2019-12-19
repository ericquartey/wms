using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for CardSensorAxisBay
    /// </summary>
    public partial class CardSensorBay : UserControl
    {
        #region Fields

        public static readonly DependencyProperty BayChainTargetPositionProperty =
            DependencyProperty.Register(nameof(BayChainTargetPosition), typeof(double?), typeof(CardSensorBay));

        private readonly IMachineService machineService;

        private readonly ISensorsService sensorsService;

        #endregion

        #region Constructors

        public CardSensorBay()
        {
            this.InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.machineService = ServiceLocator.Current.GetInstance<IMachineService>();
            this.sensorsService = ServiceLocator.Current.GetInstance<ISensorsService>();

            this.DataContext = new
            {
                MachineService = this.machineService,
                SensorsService = this.sensorsService
            };
        }

        #endregion

        #region Properties

        public double? BayChainTargetPosition
        {
            get => (double?)this.GetValue(BayChainTargetPositionProperty);
            set => this.SetValue(BayChainTargetPositionProperty, value);
        }

        #endregion
    }
}
