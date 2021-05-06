using System.IO;

namespace Ferretto.VW.App.Modules.Installation.Models
{
    public class InstallerInfo
    {
        #region Fields

        private const string ASSEMBLYNAMEPANELINSTALLER = "Ferretto.VW.Installer";

        private const string ASSEMBLYNAMEPANELPC = "Ferretto.VW.App";

        private const string ASSEMBLYNAMESERVICE = "Ferretto.VW.MAS.AutomationService";

        #endregion

        #region Constructors

        public InstallerInfo()
        {
            this.Id = 1;
        }

        public InstallerInfo(string fileName) : this()
        {
            this.FileInfo = Path.GetFileName(fileName);
            this.FileName = fileName;
        }

        public InstallerInfo(string fileName, bool isOnUsb, bool isOnMainPc) : this(fileName)
        {
            this.IsOnUsb = isOnUsb;
        }

        public InstallerInfo(string productVersion, string serviceVersion, string panelPcVersion, string fileName) : this()
        {
            this.ProductVersion = productVersion;
            this.ServiceVersion = serviceVersion;
            this.PanelPcVersion = panelPcVersion;
            this.FileName = fileName;
        }

        #endregion

        #region Properties

        public string FileInfo { get; private set; }

        public string FileName { get; set; }

        public int Id { get; private set; }

        public bool IsOnMainPc { get; }

        public bool IsOnUsb { get; }

        public bool IsValid =>
            !string.IsNullOrEmpty(this.FileName)
            &&
            !string.IsNullOrEmpty(this.PanelPcVersion)
            &&
            !string.IsNullOrEmpty(this.ProductVersion)
            &&
            !string.IsNullOrEmpty(this.ServiceVersion);

        public string PanelPcVersion { get; private set; }

        public string ProductVersion { get; private set; }

        public string ServiceVersion { get; private set; }

        #endregion

        #region Methods

        public void SetAssemblyVersion(string assemblyName, string version)
        {
            if (assemblyName == ASSEMBLYNAMEPANELINSTALLER)
            {
                this.ProductVersion = version;
            }

            if (assemblyName == ASSEMBLYNAMEPANELPC)
            {
                this.PanelPcVersion = version;
            }

            if (assemblyName == ASSEMBLYNAMESERVICE)
            {
                this.ServiceVersion = version;
            }
        }

        #endregion
    }
}
