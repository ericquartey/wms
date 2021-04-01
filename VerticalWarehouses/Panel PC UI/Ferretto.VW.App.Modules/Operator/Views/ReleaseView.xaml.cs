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

            string curDir = Directory.GetCurrentDirectory();

            this.ppcWebBrowser.Source = new Uri(curDir + "\\ReleaseNotes.html");
            //this.recRelease.LoadDocument(curDir + "\\ReleaseNotes.html", DevExpress.XtraRichEdit.DocumentFormat.Html);

            //this.recRelease.BehaviorOptions.ShowPopupMenu = DevExpress.XtraRichEdit.DocumentCapability.Disabled;
            //this.recRelease.HyperlinkOptions.ShowToolTip = false;
        }

        #endregion
    }
}
