using Ferretto.VW.Utils.Source;
using Ferretto.VW.ActionBlocks;
using Ferretto.VW.InverterDriver.Source;
using System.Windows.Input;
using System.Diagnostics;
using Ferretto.VW.InverterDriver;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.LowSpeedMovements
{
    /// <summary>
    /// Interaction logic for LSMTVerticalEngineView.xaml
    /// </summary>
    public partial class LSMTVerticalEngineView : BaseView
    {
        #region Constructors

        public LSMTVerticalEngineView()
        {
            this.InitializeComponent();
            this.DataContext = new LSMTVerticalEngineViewModel();
        }

        #endregion Constructors

        #region Methods

        private void MoveDownVerticalAxisHandler(object sender, MouseButtonEventArgs e)
        {
            if (ActionManager.PositioningDrawerInstance != null)
            {
                short targetPosition = -4096;
                ActionManager.PositioningDrawerInstance.AbsoluteMovement = false;
                InverteDriverManager.InverterDriverStaticInstance.CurrentActionType = ActionType.PositioningDrawer;
                ActionManager.PositioningDrawerInstance.MoveAlongVerticalAxisToPoint(targetPosition, 0, 0, 0, 0, 0);
            }
        }

        private void MoveUpVerticalAxisHandler(object sender, MouseButtonEventArgs e)
        {
            if (ActionManager.PositioningDrawerInstance != null)
            {
                short targetPosition = 4096;
                ActionManager.PositioningDrawerInstance.AbsoluteMovement = false;
                ActionManager.PositioningDrawerInstance.MoveAlongVerticalAxisToPoint(targetPosition, 0, 0, 0, 0, 0);
            }
        }

        private void StopVerticalAxisHandler(object sender, MouseButtonEventArgs e)
        {
            if (ActionManager.PositioningDrawerInstance != null)
            {
                ActionManager.PositioningDrawerInstance.Halt();
            }
        }

        #endregion Methods
    }
}
