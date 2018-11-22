using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews
{
    internal class GetLocalIPViewModel : BindableBase
    {
        #region Fields

        private ICommand getIPButtonCommand;
        private string localIPString = "";
        private string wideIPString = "";

        #endregion Fields

        #region Properties

        public ICommand GetIPButtonCommand => this.getIPButtonCommand ?? (this.getIPButtonCommand = new DelegateCommand(() => this.GetIPsButtonMethod()));
        public String LocalIPString { get => this.localIPString; set => this.SetProperty(ref this.localIPString, value); }
        public String WideIPString { get => this.wideIPString; set => this.SetProperty(ref this.wideIPString, value); }

        #endregion Properties

        #region Methods

        private void GetIPsButtonMethod()
        {
            var strHostName = Dns.GetHostName();
            var ipEntry = Dns.GetHostEntry(strHostName);

            foreach (var ipAddress in ipEntry.AddressList)
            {
                if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    this.LocalIPString += "\n" + ipAddress.ToString();
            }

            //var request = WebRequest.Create("http://checkip.dyndns.org");
            //var response = request.GetResponse();
            //var stream = new StreamReader(response.GetResponseStream());
            //request.Proxy = null;
            //var ipWideAddress = stream.ReadToEnd();
            //ipWideAddress.
            //    Replace("<html><head><title>Current IP Check</title></head><body>Current IP Address: ", string.Empty).
            //    Replace("</body></html>", string.Empty);
            //String.Concat(this.WideIPString, "\n");
            //String.Concat(this.WideIPString, ipWideAddress);
        }

        #endregion Methods
    }
}
