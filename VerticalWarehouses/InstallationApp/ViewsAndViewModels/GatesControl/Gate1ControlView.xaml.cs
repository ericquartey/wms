using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.InstallationApp
{
    /// <summary>
    /// Interaction logic for Gate1ControlView.xaml
    /// </summary>
    public partial class Gate1ControlView : UserControl
    {
        #region Constructors

        public Gate1ControlView()
        {
            this.InitializeComponent();
            if (DataManager.CurrentData.GeneralInfo.Type_Bay1 == 1)
            {
                this.SensorRegionContentControl.Content = new CustomControls.Controls.CustomGateControlSensorsTwoPositions();
            }
            else if (DataManager.CurrentData.GeneralInfo.Type_Bay1 == 2)
            {
                this.SensorRegionContentControl.Content = new CustomControls.Controls.CustomGateControlSensorsThreePositions();
            }
        }

        #endregion Constructors
    }
}
