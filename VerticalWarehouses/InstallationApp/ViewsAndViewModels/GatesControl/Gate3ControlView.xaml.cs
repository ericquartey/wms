using System.Windows.Controls;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.InstallationApp
{
    /// <summary>
    /// Interaction logic for Gate3ControlView.xaml
    /// </summary>
    public partial class Gate3ControlView : UserControl
    {
        #region Constructors

        public Gate3ControlView()
        {
            this.InitializeComponent();
            this.DataContext = ViewModels.Gate3ControlVMInstance;
            if (DataManager.CurrentData.GeneralInfo.Type_Bay3 == 1)
            {
                this.SensorRegionContentControl.Content = new CustomControls.Controls.CustomGateControlSensorsTwoPositions();
            }
            else if (DataManager.CurrentData.GeneralInfo.Type_Bay3 == 2)
            {
                this.SensorRegionContentControl.Content = new CustomControls.Controls.CustomGateControlSensorsThreePositions();
            }
        }

        #endregion Constructors
    }
}
