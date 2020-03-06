using System;
using System.Configuration;
using System.Linq;
using System.Windows.Input;
using System.Xml;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.Installer.Core;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.Installer.ViewModels
{
    public class InstallBayViewModel : Core.BindableBase, IOperationResult
    {
        #region Fields
        private const string APPSETTINGS = "appSettings";

        private const string APPSETTINGSBAYNUMBER = "BayNumber";

        private const string APPSETTINGSAUTOMATIONSERVICEURL = "AutomationService:Url";

        private readonly InstallationService installationService;

        private bool canProcede;
        

        private bool isSuccessful;

        private RelayCommand nextCommand;

        private RelayCommand selectBayOneCommand;

        private RelayCommand selectBayThreeCommand;

        private RelayCommand selectBayTwoCommand;

        private string selectedBayInfo;

        private Bay selectedBay;

        #endregion

        #region Constructors

        public InstallBayViewModel(InstallationService installationService)
        {
            this.installationService = installationService ?? throw new ArgumentNullException(nameof(installationService));
        }

        #endregion

        #region Properties
        public Bay SelectedBay
    {
            get => this.selectedBay;
            set => this.SetProperty(ref this.selectedBay, value);
        }

        public string SelectedBayInfo
        {
            get => this.selectedBayInfo;
            set => this.SetProperty(ref this.selectedBayInfo, value);
        }

        public bool IsBayOneVisible => (this.installationService.MasConfiguration.Machine.Bays.FirstOrDefault(b => b.Number == BayNumber.BayOne) != null);

        public bool IsBayThreeVisible => (this.installationService.MasConfiguration.Machine.Bays.FirstOrDefault(b => b.Number == BayNumber.BayThree) != null);

        public bool IsBayTwoVisible => (this.installationService.MasConfiguration.Machine.Bays.FirstOrDefault(b => b.Number == BayNumber.BayTwo) != null);

        public bool IsSuccessful => this.isSuccessful;

        public MAS.DataModels.Machine Machine => this.installationService.MasConfiguration.Machine;

        public ICommand NextCommand =>
                this.nextCommand
                ??
                (this.nextCommand = new RelayCommand(this.Next, this.CanNext));

        public ICommand SelectBayOneCommand =>
                this.selectBayOneCommand
                ??
                (this.selectBayOneCommand = new RelayCommand(() => this.SelectBay(BayNumber.BayOne), this.CanSelectBay));

        public ICommand SelectBayThreeCommand =>
                this.selectBayThreeCommand
                ??
                (this.selectBayThreeCommand = new RelayCommand(() => this.SelectBay(BayNumber.BayThree), this.CanSelectBay));

        public ICommand SelectBayTwoCommand =>
                this.selectBayTwoCommand
                ??
                (this.selectBayTwoCommand = new RelayCommand(() => this.SelectBay(BayNumber.BayTwo), this.CanSelectBay));

        public virtual string Title { get; set; }

        #endregion

        #region Methods

        public void Save()
        {
            this.isSuccessful = true;
        }

        public void SelectBay(BayNumber bayNumber)
        {
            this.canProcede = false;
            if (this.installationService.MasConfiguration.Machine.Bays.FirstOrDefault(b => b.Number == bayNumber) is Bay bayFound)
            {
                var bayIpaddress = this.GetBayIpaddress(bayFound.Number);
                this.AddAppConfig("Install:Parameter:MasIpaddress", this.installationService.MasIpAddress.ToString());
                //this.AddAppConfig("Install:Parameter:BayNumber", ((int)bayFound.Number).ToString());
                this.AddAppConfig("Install:Parameter:PpcIpaddress", bayIpaddress);                
                this.canProcede = true;
                this.SelectedBay = bayFound;
                this.SelectedBayInfo = $"Baia {(int)bayFound.Number} selezionata";
            }
            

            this.RaiseCanExecuteChanged();
        }

        private void AddAppConfig(string parameter, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                throw new ArgumentException($"On parameter {parameter}, value is null or empty");
            }

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove(parameter);
            config.AppSettings.Settings.Add(new KeyValueConfigurationElement(parameter, keyValue));
            config.Save(ConfigurationSaveMode.Modified);
        }

        private bool CanNext()
        {
            return this.canProcede;
        }

        private bool CanSelectBay()
        {
            return (this.installationService.MasConfiguration != null);
        }

        private string GetBayIpaddress(BayNumber number)
        {
            switch (number)
            {
                case BayNumber.BayOne:
                    return ConfigurationManager.AppSettings.GetInstallBay1Ipaddress();

                case BayNumber.BayTwo:
                    return ConfigurationManager.AppSettings.GetInstallBay2Ipaddress();

                case BayNumber.BayThree:
                    return ConfigurationManager.AppSettings.GetInstallBay3Ipaddress();
            }

            return null;
        }

        private void Next()
        {
            try
            {
                this.installationService.UpdateMachineRole();
                this.installationService.LoadSteps();
                this.SavePanelPcConfig();
                this.installationService.SetOperation(OperationMode.Update);
                this.isSuccessful = true;
            }
            catch
            {
            }
        }

        private void SavePanelPcConfig()
        {
            var xmlDoc = new XmlDocument();

            var panelPcFileConfig =  $"..\\{ConfigurationManager.AppSettings.GetInstallPpcPath()}\\{ConfigurationManager.AppSettings.GetIGetInstallPpcFilePath()}.config";            
            xmlDoc.Load(panelPcFileConfig);

            foreach (XmlElement element in xmlDoc.DocumentElement)
            {
                if (element.Name == APPSETTINGS)
                {
                    if (element.ChildNodes.OfType<XmlElement>().FirstOrDefault(a => a.Attributes["key"].Value == APPSETTINGSBAYNUMBER) is XmlElement bayNumberNode)
                    {
                        bayNumberNode.Attributes["value"].Value = ((int)this.selectedBay.Number).ToString();
                    }

                    if (element.ChildNodes.OfType<XmlElement>().FirstOrDefault(a => a.Attributes["key"].Value == APPSETTINGSAUTOMATIONSERVICEURL) is XmlElement ipMasNode)
                    {
                        var masIp= (this.installationService.MasIpAddress is null)? ConfigurationManager.AppSettings.GetInstallDefaultMasIpaddress() :this.installationService.MasIpAddress.ToString();                        
                        ipMasNode.Attributes["value"].Value = $"{masIp}:{ConfigurationManager.AppSettings.GetInstallDefaultMasIpport()}";
                    }
                }
            }
            
            xmlDoc.Save(panelPcFileConfig);
        }

        private void RaiseCanExecuteChanged()
        {
            this.nextCommand.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
