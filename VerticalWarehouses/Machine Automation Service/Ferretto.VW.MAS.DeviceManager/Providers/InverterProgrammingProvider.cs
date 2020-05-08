using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal sealed class InverterProgrammingProvider : BaseProvider, IInverterProgrammingProvider
    {
        #region Constructors

        public InverterProgrammingProvider(
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
        }

        #endregion

        #region Methods

        public IEnumerable<InverterParametersData> GetInvertersParametersData(IEnumerable<Inverter> inverters)
        {
            var invertersParametersData = new List<InverterParametersData>();

            foreach (var inv in inverters)
            {
                invertersParametersData.Add(new InverterParametersData((byte)inv.Index, this.GetShortInverterDescription(inv.Type, inv.IpAddress, inv.TcpPort), inv.Parameters));
            }

            return invertersParametersData;
        }

        public void Start(VertimagConfiguration vertimagConfiguration, BayNumber requestingBay, MessageActor sender)
        {
            var inverterParametersData = this.GetInvertersParameters(vertimagConfiguration);

            var inverters = inverterParametersData.OrderBy(i => i.InverterIndex);

            this.PublishCommand(
                new InverterProgrammingMessageData(inverters, CommonUtils.CommandAction.Start),
                $"Bay {requestingBay} requested Inverter programming runnning State",
                MessageActor.DeviceManager,
                sender,
                MessageType.InverterProgramming,
                requestingBay,
                requestingBay);
        }

        public void Start(VertimagConfiguration vertimagConfiguration, byte inverterIndex, BayNumber requestingBay, MessageActor sender)
        {
            var inverterParametersData = this.GetInvertersParameters(vertimagConfiguration);

            if (inverterParametersData.FirstOrDefault(i => i.InverterIndex == inverterIndex) is InverterParametersData inverterParameters
                &&
                inverterParameters.Parameters?.Count() > 0)
            {
                this.PublishCommand(
                new InverterProgrammingMessageData(new List<InverterParametersData> { inverterParameters }, CommonUtils.CommandAction.Start),
                $"Bay {requestingBay} requested Inverter programming runnning State",
                MessageActor.DeviceManager,
                sender,
                MessageType.InverterProgramming,
                requestingBay,
                requestingBay);
            }
            else
            {
                throw new ArgumentException($"No Inverter Parameters found for {inverterIndex}");
            }
        }

        public void Stop(BayNumber requestingBay, MessageActor sender)
        {
            this.PublishCommand(
                new InverterProgrammingMessageData(null, CommonUtils.CommandAction.Stop),
                $"Bay {requestingBay} requested to stop Inverter programming Running State",
                MessageActor.DeviceManager,
                sender,
                MessageType.InverterProgramming,
                requestingBay,
                requestingBay);
        }

        private IEnumerable<InverterParametersData> GetInvertersParameters(VertimagConfiguration vertimagConfiguration)
        {
            var inverterParametersData = new List<InverterParametersData>();

            foreach (var axe in vertimagConfiguration.Machine.Elevator.Axes)
            {
                if (!(axe.Inverter?.Parameters is null))
                {
                    inverterParametersData.Add(new InverterParametersData((byte)axe.Inverter.Index, this.GetShortInverterDescription(axe.Inverter.Type, axe.Inverter.IpAddress, axe.Inverter.TcpPort), axe.Inverter.Parameters));
                }
            }

            foreach (var bay in vertimagConfiguration.Machine.Bays)
            {
                if (!(bay.Inverter?.Parameters is null))
                {
                    inverterParametersData.Add(new InverterParametersData((byte)bay.Inverter.Index, this.GetShortInverterDescription(bay.Inverter.Type, bay.Inverter.IpAddress, bay.Inverter.TcpPort), bay.Inverter.Parameters));
                }
            }

            if (inverterParametersData.Count == 0)
            {
                throw new Exception("No inverters parameters found.");
            }

            return inverterParametersData;
        }

        private string GetShortInverterDescription(InverterType type, IPAddress ipAddress, int tcpPort)
        {
            var port = (tcpPort == 0) ? string.Empty : tcpPort.ToString();
            var ip = (ipAddress is null) ? string.Empty : ipAddress?.ToString();
            var ipPort = (string.IsNullOrEmpty(ip)) ? string.Empty : $"{ip}:{port}";

            return $"{type.ToString()} {ipPort}";
        }

        #endregion
    }
}
