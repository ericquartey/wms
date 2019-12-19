using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for CardSensorAxisHorizontal
    /// </summary>
    public partial class CardSensorAxisHorizontal : UserControl
    {
        #region Fields

        public static readonly DependencyProperty HorizontalTargetPositionProperty =
            DependencyProperty.Register(nameof(HorizontalTargetPosition), typeof(double?), typeof(CardSensorAxisHorizontal));

        private readonly IMachineService machineService;

        private readonly ISensorsService sensorsService;

        #endregion

        #region Constructors

        public CardSensorAxisHorizontal()
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

        public double? HorizontalTargetPosition
        {
            get => (double?)this.GetValue(HorizontalTargetPositionProperty);
            set => this.SetValue(HorizontalTargetPositionProperty, value);
        }

        #endregion
    }
}
