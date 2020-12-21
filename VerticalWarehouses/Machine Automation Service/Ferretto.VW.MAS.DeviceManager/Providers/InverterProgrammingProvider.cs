using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal sealed class InverterProgrammingProvider : BaseProvider, IInverterProgrammingProvider
    {
        #region Fields

        private readonly IDigitalDevicesDataProvider digitalDevicesDataProvider;

        #endregion

        #region Constructors

        public InverterProgrammingProvider(
            IDigitalDevicesDataProvider digitalDevicesDataProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.digitalDevicesDataProvider = digitalDevicesDataProvider ?? throw new ArgumentNullException(nameof(digitalDevicesDataProvider));
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

        //public void Start(VertimagConfiguration vertimagConfiguration, BayNumber requestingBay, MessageActor sender)
        //{
        //    var inverterParametersData = this.GetInvertersParameters(vertimagConfiguration);

        //    if (inverterParametersData.Count() == 0)
        //    {
        //        throw new ArgumentException($"No Inverters found");
        //    }

        //    this.PublishCommand(
        //        new InverterProgrammingMessageData(inverterParametersData, CommonUtils.CommandAction.Start),
        //        $"Bay {requestingBay} requested Inverter programming runnning State",
        //        MessageActor.DeviceManager,
        //        sender,
        //        MessageType.InverterProgramming,
        //        requestingBay,
        //        requestingBay);
        //}

        //public void Start(VertimagConfiguration vertimagConfiguration, byte inverterIndex, BayNumber requestingBay, MessageActor sender)
        //{
        //    var inverterParametersData = this.GetInvertersParameters(vertimagConfiguration);

        //    var inverterParameters = inverterParametersData.Where(i => i.InverterIndex == inverterIndex).ToList();

        //    if (inverterParameters is null)
        //    {
        //        throw new ArgumentException($"No Inverter found");
        //    }

        //    if (inverterParameters.Skip(1).FirstOrDefault()?.Parameters?.Count() == 0)
        //    {
        //        throw new ArgumentException($"No Inverter Parameters found for {inverterIndex}");
        //    }

        //    this.PublishCommand(
        //        new InverterProgrammingMessageData(inverterParameters, CommonUtils.CommandAction.Start),
        //        $"Bay {requestingBay} requested Inverter programming runnning State",
        //        MessageActor.DeviceManager,
        //        sender,
        //        MessageType.InverterProgramming,
        //        requestingBay,
        //        requestingBay);
        //}

        public void Read(IEnumerable<Inverter> inverters, BayNumber requestingBay, MessageActor sender)
        {
            //check inverters
            foreach (var inverter in inverters)
            {
                var result = this.digitalDevicesDataProvider.CheckInverterParametersValidity(inverter.Index);
                if (result)
                {
                    throw new ArgumentException($"No Inverters parameters found");
                }
            }

            //send command
        }

        public void Read(Inverter inverter, BayNumber requestingBay, MessageActor sender)
        {
            //check inverter
            var result = this.digitalDevicesDataProvider.CheckInverterParametersValidity(inverter.Index);
            if (result)
            {
                throw new ArgumentException($"No Inverter parameters found");
            }

            //send command
        }

        public void Start(IEnumerable<Inverter> inverters, BayNumber requestingBay, MessageActor sender)
        {
            var inverterParametersCheckVersionData = new List<InverterParametersData>();
            var inverterParametersWriteData = new List<InverterParametersData>();

            foreach (var inverter in inverters)
            {
                var newInverterParametersData = this.GetInverterParameters(inverter);
                inverterParametersCheckVersionData.Add(newInverterParametersData.inverterParametersCheckVersionData);
                inverterParametersWriteData.Add(newInverterParametersData.inverterParametersWriteData);
            }

            var inverterParametersData = new List<InverterParametersData>();
            inverterParametersData.AddRange(inverterParametersCheckVersionData);
            inverterParametersData.AddRange(inverterParametersWriteData.OrderBy(i => i.InverterIndex).ToList());

            if (!inverterParametersData.Any())
            {
                throw new ArgumentException($"No Inverters found");
            }

            this.PublishCommand(
                new InverterProgrammingMessageData(inverterParametersData, CommonUtils.CommandAction.Start),
                $"Bay {requestingBay} requested Inverter programming runnning State",
                MessageActor.DeviceManager,
                sender,
                MessageType.InverterProgramming,
                requestingBay,
                requestingBay);
        }

        public void Start(Inverter inverter, BayNumber requestingBay, MessageActor sender)
        {
            var newInverterParametersData = this.GetInverterParameters(inverter);

            var inverterParametersData = new List<InverterParametersData>();
            inverterParametersData.Add(newInverterParametersData.inverterParametersCheckVersionData);
            inverterParametersData.Add(newInverterParametersData.inverterParametersWriteData);

            if (inverterParametersData is null)
            {
                throw new ArgumentException($"No Inverter found");
            }

            if (inverterParametersData.Skip(1).FirstOrDefault()?.Parameters?.Count() == 0)
            {
                throw new ArgumentException($"No Inverter Parameters found for {inverterParametersData.FirstOrDefault().InverterIndex}");
            }

            this.PublishCommand(
                new InverterProgrammingMessageData(inverterParametersData, CommonUtils.CommandAction.Start),
                $"Bay {requestingBay} requested Inverter programming runnning State",
                MessageActor.DeviceManager,
                sender,
                MessageType.InverterProgramming,
                requestingBay,
                requestingBay);
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

        private (InverterParametersData inverterParametersCheckVersionData, InverterParametersData inverterParametersWriteData) GetInverterParameters(Inverter inverter)
        {
            var inverterParametersWriteData = new InverterParametersData((byte)inverter.Index,
                                            this.GetShortInverterDescription(inverter.Type,
                                            inverter.IpAddress, inverter.TcpPort),
                                            this.GetWritableParameters(inverter.Parameters));

            var inverterParametersCheckVersionData = this.InverterVersionParameterData(inverter);

            return (inverterParametersCheckVersionData, inverterParametersWriteData);
        }

        private string GetShortInverterDescription(InverterType type, IPAddress ipAddress, int tcpPort)
        {
            var port = (tcpPort == 0) ? string.Empty : tcpPort.ToString();
            var ip = (ipAddress is null) ? string.Empty : ipAddress?.ToString();
            var ipPort = (string.IsNullOrEmpty(ip)) ? string.Empty : $"{ip}:{port}";

            return $"{type.ToString()} {ipPort}";
        }

        private IEnumerable<InverterParameter> GetWritableParameters(IEnumerable<InverterParameter> parameters)
        {
            return parameters.Where(p => !p.IsReadOnly);
        }

        private InverterParametersData InverterVersionParameterData(Inverter inverter)
        {
            var parameters = new List<InverterParameter>();
            var versionInverterParameter = new InverterParameter
            {
                Code = (short)InverterParameterId.SoftwareVersion,
                DataSet = 0,
                Type = "String",
                StringValue = ((InverterParameter)inverter.Parameters.SingleOrDefault(p => ((InverterParameter)p).Code == (short)InverterParameterId.SoftwareVersion)).StringValue
            };
            parameters.Add(versionInverterParameter);
            return new InverterParametersData((byte)inverter.Index, null, parameters, true);
        }

        #endregion
    }
}
