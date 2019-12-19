using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for CardSensorAxisVertical
    /// </summary>
    public partial class CardSensorAxisVertical : UserControl
    {
        #region Fields

        public static readonly DependencyProperty VerticalTargetPositionProperty =
            DependencyProperty.Register(nameof(VerticalTargetPosition), typeof(double?), typeof(CardSensorAxisVertical));

        private readonly IMachineService machineService;

        #endregion

        #region Constructors

        public CardSensorAxisVertical()
        {
            this.InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.machineService = ServiceLocator.Current.GetInstance<IMachineService>();

            this.DataContext = this.machineService;
        }

        #endregion

        #region Properties

        public double? VerticalTargetPosition
        {
            get => (double?)this.GetValue(VerticalTargetPositionProperty);
            set => this.SetValue(VerticalTargetPositionProperty, value);
        }

        #endregion
    }
}
