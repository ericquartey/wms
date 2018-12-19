using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.GatesControl
{
    /// <summary>
    /// Interaction logic for Gate2ControlView.xaml
    /// </summary>
    public partial class Gate2ControlView : UserControl
    {
        #region Constructors

        public Gate2ControlView()
        {
            this.InitializeComponent();
            this.DataContext = ViewModels.Gate2ControlVMInstance;
            if (DataManager.CurrentData.GeneralInfo.Type_Bay2 == 1)
            {
                this.SensorRegionContentControl.Content = new CustomControls.Controls.CustomGateControlSensorsTwoPositions();
            }
            else if (DataManager.CurrentData.GeneralInfo.Type_Bay2 == 2)
            {
                this.SensorRegionContentControl.Content = new CustomControls.Controls.CustomGateControlSensorsThreePositions();
            }
        }

        #endregion Constructors
    }
}
