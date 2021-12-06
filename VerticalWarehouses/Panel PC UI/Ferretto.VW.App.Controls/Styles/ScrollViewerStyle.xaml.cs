using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Styles
{
    public partial class ScrollViewerStyle : ResourceDictionary
    {
        #region Fields

        private IMachineIdentityWebService machineIdentityWebService = null;

        #endregion

        #region Constructors

        public ScrollViewerStyle()
        {
        }

        #endregion

        #region Methods

        private async void UpDownButton_Loaded(object sender, EventArgs e)
        {
            if (this.machineIdentityWebService is null)
            {
                this.machineIdentityWebService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IMachineIdentityWebService>();
            }

            var touchHelperEnabled = await this.machineIdentityWebService.GetTouchHelperEnableAsync();

            var button = sender as RepeatButton;
            button.Height = touchHelperEnabled ? 50 : 25;
        }

        #endregion
    }
}
