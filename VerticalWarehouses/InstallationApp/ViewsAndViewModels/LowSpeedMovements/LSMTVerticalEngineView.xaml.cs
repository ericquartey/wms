using System.Windows.Input;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.InstallationApp
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
        }

        #endregion Constructors

        #region Methods

        private void MoveDownVerticalAxisHandler(object sender, MouseButtonEventArgs e)
        {
            if (((LSMTVerticalEngineViewModel)this.DataContext).PositioningDrawer != null)
            {
                short targetPosition = -4096;
                ((LSMTVerticalEngineViewModel)this.DataContext).PositioningDrawer.AbsoluteMovement = false;
                ((LSMTVerticalEngineViewModel)this.DataContext).PositioningDrawer.MoveAlongVerticalAxisToPoint(targetPosition, 0, 0, 0, 0, 0);
            }
        }

        private void MoveUpVerticalAxisHandler(object sender, MouseButtonEventArgs e)
        {
            if (((LSMTVerticalEngineViewModel)this.DataContext).PositioningDrawer != null)
            {
                short targetPosition = 4096;
                ((LSMTVerticalEngineViewModel)this.DataContext).PositioningDrawer.AbsoluteMovement = false;
                ((LSMTVerticalEngineViewModel)this.DataContext).PositioningDrawer.MoveAlongVerticalAxisToPoint(targetPosition, 0, 0, 0, 0, 0);
            }
        }

        private void StopVerticalAxisHandler(object sender, MouseButtonEventArgs e)
        {
            if (((LSMTVerticalEngineViewModel)this.DataContext).PositioningDrawer != null)
            {
                ((LSMTVerticalEngineViewModel)this.DataContext).PositioningDrawer.Stop();
            }
        }

        #endregion Methods
    }
}
