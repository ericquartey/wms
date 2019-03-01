using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Controls;
using System.Windows.Input;
using Ferretto.VW.InstallationApp.Resources.Enumerables;
using Newtonsoft.Json;

namespace Ferretto.VW.InstallationApp
{
    public partial class LSMTVerticalEngineView : UserControl
    {
        #region Constructors

        public LSMTVerticalEngineView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private async void MoveDownVerticalAxisHandler(object sender, MouseButtonEventArgs e)
        {
            var values = new Dictionary<string, string>
            {
                { "mm", JsonConvert.SerializeObject(-100m) },
                { "axis", JsonConvert.SerializeObject(1) },
                { "movementType", JsonConvert.SerializeObject(0) },
                { "speedPercentage", JsonConvert.SerializeObject(50) }
            };
            var content = new FormUrlEncodedContent(values);
            await new HttpClient().PostAsync(new Uri("http://localhost:5000/api/Installation/ExecuteMovement"), content);
        }

        private async void MoveUpVerticalAxisHandler(object sender, MouseButtonEventArgs e)
        {
            var values = new Dictionary<string, string>
            {
                { "mm", JsonConvert.SerializeObject(100m) },
                { "axis", JsonConvert.SerializeObject(1) },
                { "movementType", JsonConvert.SerializeObject(0) },
                { "speedPercentage", JsonConvert.SerializeObject(50u) }
            };
            var content = new FormUrlEncodedContent(values);
            await new HttpClient().PostAsync(new Uri("http://localhost:5000/api/Installation/ExecuteMovement"), content);
        }

        private void StopVerticalAxisHandler(object sender, MouseButtonEventArgs e)
        {
            new HttpClient().GetAsync("http://localhost:5000/api/Installation/StopCommand");
        }

        #endregion
    }
}
