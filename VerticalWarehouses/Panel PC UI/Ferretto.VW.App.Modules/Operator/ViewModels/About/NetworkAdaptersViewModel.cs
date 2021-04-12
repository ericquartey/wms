using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using System.Diagnostics;
using Prism.Commands;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Resources;
using System.Threading;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class NetworkAdapter
    {
        #region Constructors

        public NetworkAdapter()
        {
        }

        #endregion

        #region Properties

        public string Description { get; set; }

        public bool DHCP { get; set; }

        public string DNS1 { get; set; }

        public string DNS2 { get; set; }

        public string Gateway { get; set; }

        public string Gateway_Additional { get; set; }

        public string IP { get; set; }

        public string IP_Additional { get; set; }

        public string Name { get; set; }

        public string SubnetMask { get; set; }

        public string SubnetMask_Additional { get; set; }

        #endregion
    }

    [Warning(WarningsArea.Information)]
    internal sealed class NetworkAdaptersViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private readonly ISessionService sessionService;

        private bool isBusy;

        private bool isFilterEnabled;

        private NetworkAdapter networkAdapter0;

        private NetworkAdapter networkAdapter1;

        private DelegateCommand saveNetworkAdapter0;

        private DelegateCommand saveNetworkAdapter1;

        private DelegateCommand uwfOffCommand;

        private DelegateCommand uwfOnCommand;

        #endregion

        #region Constructors

        public NetworkAdaptersViewModel(ISessionService sessionService)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }

        #endregion

        #region Properties

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public bool IsEditEnabled => this.sessionService.UserAccessLevel >= MAS.AutomationService.Contracts.UserAccessLevel.Installer;

        public bool IsFilterEnabled
        {
            get => this.isFilterEnabled;
            set => this.SetProperty(ref this.isFilterEnabled, value);
        }

        public NetworkAdapter NetworkAdapter0
        {
            get => this.networkAdapter0;
            set => this.SetProperty(ref this.networkAdapter0, value);
        }

        public NetworkAdapter NetworkAdapter1
        {
            get => this.networkAdapter1;
            set => this.SetProperty(ref this.networkAdapter1, value);
        }

        public ICommand SaveNetworkAdapter0 => this.saveNetworkAdapter0
                                          ??
                  (this.saveNetworkAdapter0 = new DelegateCommand(
                      () => this.SetConfig(this.networkAdapter0)));

        public ICommand SaveNetworkAdapter1 => this.saveNetworkAdapter1
                          ??
                  (this.saveNetworkAdapter1 = new DelegateCommand(
                      () => this.SetConfig(this.networkAdapter1)));

        public ICommand UwfOffCommand => this.uwfOffCommand
                                          ??
                  (this.uwfOffCommand = new DelegateCommand(
                      () => this.SetUWF(false)));

        public ICommand UwfOnCommand => this.uwfOnCommand
                          ??
                  (this.uwfOnCommand = new DelegateCommand(
                      () => this.SetUWF(true)));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.IsBusy = true;

            this.RaisePropertyChanged(nameof(this.IsEditEnabled));

            this.IsBackNavigationAllowed = true;

            await base.OnAppearedAsync();

            this.GetNetworkAdapters();

            this.IsBusy = false;

            this.CheckUwf();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.saveNetworkAdapter0.RaiseCanExecuteChanged();
            this.saveNetworkAdapter1.RaiseCanExecuteChanged();
            this.uwfOffCommand.RaiseCanExecuteChanged();
            this.uwfOnCommand.RaiseCanExecuteChanged();
        }

        private static NetworkAdapter SetData(NetworkInterface networkInterface)
        {
            var networkAdapter = new NetworkAdapter();

            var properties = networkInterface.GetIPProperties();

            networkAdapter.Name = networkInterface.Name;

            networkAdapter.Description = networkInterface.Description;

            if (!properties.DhcpServerAddresses.Any())
            {
                networkAdapter.DHCP = false;
            }
            else
            {
                networkAdapter.DHCP = true;
            }

            if (properties.GatewayAddresses.Any())
            {
                if (properties.GatewayAddresses.Count == 2)
                {
                    networkAdapter.Gateway = properties.GatewayAddresses.FirstOrDefault().Address.ToString();
                    networkAdapter.Gateway_Additional = properties.GatewayAddresses.LastOrDefault().Address.ToString();
                }
                else
                {
                    networkAdapter.Gateway = properties.GatewayAddresses.FirstOrDefault().Address.ToString();
                }
            }

            var isSetPrincipalIP = false;
            foreach (UnicastIPAddressInformation ip in properties.UnicastAddresses)
            {
                if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    if (!isSetPrincipalIP)
                    {
                        networkAdapter.IP = ip.Address.ToString();
                        networkAdapter.SubnetMask = ip.IPv4Mask.ToString();
                        isSetPrincipalIP = true;
                    }
                    else
                    {
                        networkAdapter.IP_Additional = ip.Address.ToString();
                        networkAdapter.SubnetMask_Additional = ip.IPv4Mask.ToString();
                    }
                }
            }

            if (properties.DnsAddresses.Any())
            {
                if (properties.DnsAddresses.Count == 2)
                {
                    networkAdapter.DNS1 = properties.DnsAddresses.FirstOrDefault().ToString();
                    networkAdapter.DNS2 = properties.DnsAddresses.LastOrDefault().ToString();
                }
                else
                {
                    networkAdapter.DNS1 = properties.DnsAddresses.FirstOrDefault().ToString();
                }
            }

            return networkAdapter;
        }

        private void CheckUwf()
        {
            try
            {
                Process p = new Process();

                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.Verb = "runas";
                p.StartInfo.Arguments = "/c uwfmgr.exe get-config";
                p.Start();

                // standard output contains many null chars: remove them!
                var rawOutput = p.StandardOutput.ReadToEnd().ToCharArray();
                var output = string.Empty;
                foreach (var singleChar in rawOutput)
                {
                    if (char.IsLetterOrDigit(singleChar))
                    {
                        output += singleChar;
                    }
                }

                output = output.ToUpperInvariant();

                //this.Logger.Debug(output);
                this.IsFilterEnabled = output.Contains("STATOFILTROATTIVATA") || output.Contains("FILTERSTATEON");

                p.WaitForExit();
            }
            catch (Exception ex)
            {
            }

            this.ClearNotifications();
            if (this.isFilterEnabled)
            {
                this.ShowNotification("Filtro UWF attivo", Services.Models.NotificationSeverity.Warning);
            }
            else
            {
                this.ShowNotification("Filtro UWF NON attivo", Services.Models.NotificationSeverity.Warning);
            }
        }

        private void GetNetworkAdapters()
        {
            var allnetworkAdapter = NetworkInterface.GetAllNetworkInterfaces();
            var ethAdapters = allnetworkAdapter.Where(s => s.NetworkInterfaceType == NetworkInterfaceType.Ethernet);

            var isSetNetwork0 = false;
            foreach (var ethAdapter in ethAdapters)
            {
                var adapter = SetData(ethAdapter);

                if (!isSetNetwork0)
                {
                    this.NetworkAdapter0 = adapter;
                    isSetNetwork0 = true;
                }
                else
                {
                    this.NetworkAdapter1 = adapter;
                }
            }

            this.RaisePropertyChanged(nameof(this.NetworkAdapter0));
            this.RaisePropertyChanged(nameof(this.NetworkAdapter1));

            this.ShowNotification(Localized.Get("InstallationApp.InformationSuccessfullyUpdated"), Services.Models.NotificationSeverity.Success);
        }

        private void SendPromptCommand(string arg)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe");

                psi.UseShellExecute = true;

                psi.WindowStyle = ProcessWindowStyle.Hidden;

                psi.Verb = "runas";

                psi.Arguments = arg;

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex.Message, Services.Models.NotificationSeverity.Error);
            }
        }

        private void SetConfig(NetworkAdapter networkAdapter)
        {
            this.ClearNotifications();

            if (networkAdapter.DHCP)
            {
                this.SendPromptCommand("/c netsh interface ip set address \"" + networkAdapter.Name + "\" dhcp ");

                this.SendPromptCommand("/c netsh interface ip set dns \"" + networkAdapter.Name + "\" dhcp ");
            }
            else
            {
                if (networkAdapter.Gateway is null)
                {
                    this.SendPromptCommand("/c netsh interface ip set address \"" + networkAdapter.Name + "\" static " + networkAdapter.IP + " " + networkAdapter.SubnetMask);
                }
                else
                {
                    this.SendPromptCommand("/c netsh interface ip set address \"" + networkAdapter.Name + "\" static " + networkAdapter.IP + " " + networkAdapter.SubnetMask + " " + networkAdapter.Gateway);
                }

                if (networkAdapter.Gateway_Additional is null)
                {
                    if (networkAdapter.IP_Additional != null && networkAdapter.SubnetMask_Additional != null)
                    {
                        if (networkAdapter.Gateway is null)
                        {
                            this.SendPromptCommand("/c netsh interface ipv4 add address \"" + networkAdapter.Name + "\" " + networkAdapter.IP_Additional + " " + networkAdapter.SubnetMask_Additional);
                        }
                        else
                        {
                            this.SendPromptCommand("/c netsh interface ipv4 add address \"" + networkAdapter.Name + "\" " + networkAdapter.IP_Additional + " " + networkAdapter.SubnetMask_Additional + " " + networkAdapter.Gateway_Additional);
                        }
                    }
                }
                else
                {
                    if (networkAdapter.IP_Additional != null && networkAdapter.SubnetMask_Additional != null)
                    {
                        this.SendPromptCommand("/c netsh interface ipv4 add address \"" + networkAdapter.Name + "\" " + networkAdapter.IP_Additional + " " + networkAdapter.SubnetMask_Additional + " " + networkAdapter.Gateway_Additional);
                    }
                }

                if (networkAdapter.DNS1 is null)
                {
                    this.SendPromptCommand("/c netsh interface ip set dns \"" + networkAdapter.Name + "\" static ");
                }
                else
                {
                    this.SendPromptCommand("/c netsh interface ip set dns \"" + networkAdapter.Name + "\" static " + networkAdapter.DNS1);
                }

                if (networkAdapter.DNS2 is null)
                {
                    this.SendPromptCommand("/c netsh interface ip add dns \"" + networkAdapter.Name + "\" index=2");
                }
                else
                {
                    this.SendPromptCommand("/c netsh interface ip add dns \"" + networkAdapter.Name + "\" " + networkAdapter.DNS2 + " index=2");
                }
            }

            Thread.Sleep(1000);

            this.IsBusy = true;

            this.GetNetworkAdapters();

            this.IsBusy = false;
        }

        private void SetUWF(bool on)
        {
            if (on)
            {
                this.SendPromptCommand("/c uwfmgr.exe filter enable");
            }
            else
            {
                this.SendPromptCommand("/c uwfmgr.exe filter disable");
            }

            //wait 1s
            Thread.Sleep(1000);

            var psi = new ProcessStartInfo("shutdown", "/r /t 0");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        #endregion
    }
}
