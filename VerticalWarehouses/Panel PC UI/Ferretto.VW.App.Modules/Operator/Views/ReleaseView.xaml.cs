using System;
using System.Diagnostics;
using System.IO;

namespace Ferretto.VW.App.Modules.Operator.Views
{
    public partial class ReleaseView
    {
        #region Constructors

        public ReleaseView()
        {
            this.InitializeComponent();

            string curDir = AppDomain.CurrentDomain.BaseDirectory;

            this.ppcWebBrowser.Source = new Uri(curDir + "ReleaseNotes.html");
        }

        #endregion
    }
}
