using System.ComponentModel;
using System.Windows.Controls;
using CommonServiceLocator;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for CardSensorPosition
    /// </summary>
    public partial class CardSensorPosition : UserControl
    {
        #region Fields

        private readonly IMachineService machineService;

        #endregion

        #region Constructors

        public CardSensorPosition()
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
    }
}
