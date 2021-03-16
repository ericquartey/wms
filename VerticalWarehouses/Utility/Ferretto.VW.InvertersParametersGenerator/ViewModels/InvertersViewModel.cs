using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ferretto.VW.InvertersParametersGenerator.Interfaces;
using Ferretto.VW.InvertersParametersGenerator.Models;
using Ferretto.VW.InvertersParametersGenerator.Services;
using Ferretto.VW.MAS.DataModels;
using Prism.Mvvm;

namespace Ferretto.VW.InvertersParametersGenerator.ViewModels
{
    internal sealed class InvertersViewModel : BindableBase, IOperationResult
    {
        #region Fields

        private readonly ConfigurationService configurationService;

        private readonly IParentActionChanged parentActionChanged;

        private IEnumerable<InverterParametersDataInfo> invertersParameters;

        private bool isBusy;

        private string selectedFileConfigurationName;

        #endregion Fields

        #region Constructors

        public InvertersViewModel(ConfigurationService installationService, IParentActionChanged parentActionChanged)
        {
            this.configurationService = installationService ?? throw new ArgumentNullException(nameof(installationService));
            this.parentActionChanged = parentActionChanged;
            this.LoadConfiguration();
        }

        #endregion Constructors

        #region Properties

        public bool CanNext => true;

        public bool CanPrevious => true;

        public IEnumerable<InverterParametersDataInfo> InvertersParameters => this.invertersParameters;

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string SelectedFileConfigurationName
        {
            get => this.selectedFileConfigurationName;
            set => this.SetProperty(ref this.selectedFileConfigurationName, value);
        }

        #endregion Properties

        #region Methods

        public bool Next()
        {
            this.configurationService.SetInvertersConfiguration(this.invertersParameters);
            this.configurationService.SetWizard(WizardMode.Parameters);
            return true;
        }

        public void Previous()
        {
            this.configurationService.SetWizard(WizardMode.ImportConfiguration);
        }

        private IEnumerable<InverterParametersDataInfo> GetInvertersParameters(VertimagConfiguration vertimagConfiguration)
        {
            var inverterParametersData = new List<InverterParametersDataInfo>();

            foreach (var axe in vertimagConfiguration.Machine.Elevator.Axes)
            {
                if (!(axe.Inverter is null))
                {
                    inverterParametersData.Add(new InverterParametersDataInfo(axe.Inverter.Type, (byte)axe.Inverter.Index, this.GetShortInverterDescription(axe.Inverter.IpAddress, axe.Inverter.TcpPort)));
                }
            }

            foreach (var bay in vertimagConfiguration.Machine.Bays)
            {
                if (!(bay.Inverter is null))
                {
                    inverterParametersData.Add(new InverterParametersDataInfo(bay.Inverter.Type, (byte)bay.Inverter.Index, this.GetShortInverterDescription(bay.Inverter.IpAddress, bay.Inverter.TcpPort)));
                }

                if (!(bay.Shutter?.Inverter is null))
                {
                    inverterParametersData.Add(new InverterParametersDataInfo(bay.Shutter.Inverter.Type, (byte)bay.Shutter.Inverter.Index, this.GetShortInverterDescription(bay.Shutter.Inverter.IpAddress, bay.Shutter.Inverter.TcpPort)));
                }
            }

            if (inverterParametersData.Count == 0)
            {
                throw new Exception("No inverters parameters found.");
            }

            return inverterParametersData;
        }

        private string GetShortInverterDescription(IPAddress ipAddress, int tcpPort)
        {
            var port = (tcpPort == 0) ? string.Empty : tcpPort.ToString();
            var ip = (ipAddress is null) ? string.Empty : ipAddress?.ToString();
            return (string.IsNullOrEmpty(ip)) ? string.Empty : $"{ip}:{port}";
        }

        private void LoadConfiguration()
        {
            try
            {
                this.IsBusy = true;

                if (!(this.configurationService.VertimagConfiguration is null))
                {
                    this.invertersParameters = this.GetInvertersParameters(this.configurationService.VertimagConfiguration).OrderBy(i => i.InverterIndex);
                }

                this.RaisePropertyChanged(nameof(this.InvertersParameters));
            }
            catch (Exception ex)
            {
                this.parentActionChanged.Notify(ex, NotificationSeverity.Error);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.parentActionChanged.RaiseCanExecuteChanged();
        }

        #endregion Methods
    }
}
