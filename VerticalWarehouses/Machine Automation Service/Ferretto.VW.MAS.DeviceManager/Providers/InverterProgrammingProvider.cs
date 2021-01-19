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
                invertersParametersData.Add(new InverterParametersData((byte)inv.Index, GetShortInverterDescription(inv.Type, inv.IpAddress, inv.TcpPort), inv.Parameters));
            }

            return invertersParametersData;
        }

        public void HardReset(Inverter inverter, BayNumber requestingBay, MessageActor sender)
        {
            //Hard Reset, tutta la memoria RAM viene resettata perché avviene un reboot della scheda di controllo. Corrisponde ad un riavvio.
            var inverterParametersData = new List<InverterParametersData>();

            var reset = InverterProgramData((byte)inverter.Index, "123");
            inverterParametersData.Add(reset);

            this.PublishCommand(
                new InverterProgrammingMessageData(inverterParametersData),
                $"Bay {requestingBay} requested Inverter programming runnning State",
                MessageActor.DeviceManager,
                sender,
                MessageType.InverterProgramming,
                requestingBay,
                requestingBay);
        }

        public void Read(BayNumber requestingBay, MessageActor sender)
        {
            var invetersFormDb = this.digitalDevicesDataProvider.GetAllInverters();

            //check inverters
            foreach (var inverter in invetersFormDb)
            {
                var result = this.digitalDevicesDataProvider.CheckInverterParametersValidity(inverter.Index);
                if (!result)
                {
                    throw new ArgumentException($"No Inverters parameters found");
                }
            }

            var inverterParametersData = new List<InverterParametersData>();

            foreach (var inverter in invetersFormDb)
            {
                var newInverterParametersData = this.GetInverterParameters(inverter, true);
                inverterParametersData.Add(newInverterParametersData);
            }

            if (!inverterParametersData.Any())
            {
                throw new ArgumentException($"No Inverters found");
            }

            //send command

            this.PublishCommand(
                new InverterReadingMessageData(inverterParametersData),
                $"Bay {requestingBay} requested Inverter programming runnning State",
                MessageActor.DeviceManager,
                sender,
                MessageType.InverterReading,
                requestingBay,
                requestingBay);
        }

        public void Read(InverterIndex inverterIndex, BayNumber requestingBay, MessageActor sender)
        {
            var inveterFormDb = this.digitalDevicesDataProvider.GetInverterByIndex(inverterIndex);

            //check inverters
            var result = this.digitalDevicesDataProvider.CheckInverterParametersValidity(inverterIndex);
            if (result)
            {
                throw new ArgumentException($"No Inverter parameters found");
            }
            var newInverterParametersData = this.GetInverterParameters(inveterFormDb, true);

            var inverterParametersData = new List<InverterParametersData>();
            inverterParametersData.Add(newInverterParametersData);

            if (!inverterParametersData.Any())
            {
                throw new ArgumentException($"No Inverters found");
            }

            //send command

            this.PublishCommand(
                new InverterReadingMessageData(inverterParametersData),
                $"Bay {requestingBay} requested Inverter programming runnning State",
                MessageActor.DeviceManager,
                sender,
                MessageType.InverterReading,
                requestingBay,
                requestingBay);
        }

        public void Reset(Inverter inverter, BayNumber requestingBay, MessageActor sender)
        {
            //Reset ai valori di fabbrica. Viene cancellata tutta la memoria e la parametrizzazione inverter. Gli unici parametri che non vengono riportati ai valori di fabbrica sono il P28 e il P30.
            var inverterParametersData = new List<InverterParametersData>();

            var reset = InverterProgramData((byte)inverter.Index, "4444");
            inverterParametersData.Add(reset);

            this.PublishCommand(
                new InverterProgrammingMessageData(inverterParametersData),
                $"Bay {requestingBay} requested Inverter programming runnning State",
                MessageActor.DeviceManager,
                sender,
                MessageType.InverterProgramming,
                requestingBay,
                requestingBay);
        }

        public void Start(IEnumerable<Inverter> inverters, BayNumber requestingBay, MessageActor sender)
        {
            var inverterParametersData = new List<InverterParametersData>();

            foreach (var inverter in inverters.OrderBy(s => s.Index))
            {
                var newInverterParametersData = this.GetInverterParameters(inverter, false, false);
                inverterParametersData.Add(newInverterParametersData);
            }

            if (!inverterParametersData.Any())
            {
                throw new ArgumentException($"No Inverters found");
            }

            this.PublishCommand(
                new InverterProgrammingMessageData(inverterParametersData),
                $"Bay {requestingBay} requested Inverter programming runnning State",
                MessageActor.DeviceManager,
                sender,
                MessageType.InverterProgramming,
                requestingBay,
                requestingBay);
        }

        public void Start(Inverter inverter, BayNumber requestingBay, MessageActor sender)
        {
            var newInverterParametersData = this.GetInverterParameters(inverter, false, false);

            var inverterParametersData = new List<InverterParametersData>();
            inverterParametersData.Add(newInverterParametersData);

            if (inverterParametersData is null)
            {
                throw new ArgumentException($"No Inverter found");
            }

            if (inverterParametersData.Skip(1).FirstOrDefault()?.Parameters?.Count() == 0)
            {
                throw new ArgumentException($"No Inverter Parameters found for {inverterParametersData.FirstOrDefault().InverterIndex}");
            }

            this.PublishCommand(
                new InverterProgrammingMessageData(inverterParametersData),
                $"Bay {requestingBay} requested Inverter programming runnning State",
                MessageActor.DeviceManager,
                sender,
                MessageType.InverterProgramming,
                requestingBay,
                requestingBay);
        }

        private static string GetShortInverterDescription(InverterType type, IPAddress ipAddress, int tcpPort)
        {
            var port = (tcpPort == 0) ? string.Empty : tcpPort.ToString();
            var ip = (ipAddress is null) ? string.Empty : ipAddress?.ToString();
            var ipPort = (string.IsNullOrEmpty(ip)) ? string.Empty : $"{ip}:{port}";

            return $"{type.ToString()} {ipPort}";
        }

        private static InverterParametersData InverterProgramData(byte inverterIndex, string action)
        {
            var parameters = new List<InverterParameter>();
            var versionInverterParameter = new InverterParameter
            {
                Code = (short)InverterParameterId.Program,
                DataSet = 0,
                Type = "short",
                Description = "Program",
                StringValue = action
            };
            parameters.Add(versionInverterParameter);
            return new InverterParametersData(inverterIndex, null, parameters, false);
        }

        private static InverterParameter InverterRunModeParameterData(bool state)
        {
            return new InverterParameter
            {
                Code = (short)InverterParameterId.RunMode,
                DataSet = 0,
                Type = "short",
                Description = "Run mode",
                StringValue = state ? "0" : "1"
            };
        }

        private List<InverterParameter> FixParameterList(IEnumerable<InverterParameter> parameters, bool read, InverterIndex inverterIndex)
        {
            var parametersNew = new List<InverterParameter>();

            parametersNew.Add(InverterRunModeParameterData(false));

            if (read)
            {
                foreach (var parameter in parameters)
                {
                    if (parameter.Code == (short)InverterParameterId.RunMode)
                    {
                        continue;
                    }
                    if (parameter.ReadCode != 0)
                    {
                        var basePara = parameters.FirstOrDefault(s => s.Code == parameter.ReadCode);

                        if (basePara is null)
                        {
                            if (this.digitalDevicesDataProvider.ExistInverterParameter(inverterIndex, parameter.ReadCode, 0))
                            {
                                throw new ArgumentException($"Read parameters not found");
                            }
                            basePara = this.digitalDevicesDataProvider.GetParameter(inverterIndex, parameter.ReadCode, 0);
                        }
                        basePara.DataSet = parameter.DataSet;
                        parametersNew.Add(basePara);
                        parametersNew.Add(parameter);
                    }
                    else
                    {
                        parametersNew.Add(parameter);
                    }
                }
            }
            else
            {
                foreach (var parameter in parameters)
                {
                    if (parameter.WriteCode != 0)
                    {
                        var basePara = parameters.FirstOrDefault(s => s.Code == parameter.WriteCode);

                        if (basePara is null)
                        {
                            if (!this.digitalDevicesDataProvider.ExistInverterParameter(inverterIndex, parameter.WriteCode, 0))
                            {
                                throw new ArgumentException($"Write parameters not found");
                            }
                            basePara = this.digitalDevicesDataProvider.GetParameter(inverterIndex, parameter.WriteCode, 0);
                        }
                        var writeParameter = new InverterParameter
                        {
                            Code = basePara.Code,
                            DataSet = 0,
                            Type = basePara.Type,
                            StringValue = parameter.DataSet.ToString()
                        };
                        parametersNew.Add(writeParameter);
                        parameter.DataSet = 0;
                        parametersNew.Add(parameter);
                    }
                    else
                    {
                        parametersNew.Add(parameter);
                    }
                }
            }

            parametersNew.Add(InverterRunModeParameterData(true));

            return parametersNew;
        }

        private InverterParametersData GetInverterParameters(Inverter inverter, bool read, bool takeReadOnly = true)
        {
            var fixedParameters = this.FixParameterList(inverter.Parameters, read, inverter.Index);

            if (!takeReadOnly)
            {
                fixedParameters = fixedParameters.Where(s => s.IsReadOnly == false).ToList();
            }

            var inverterParametersWriteData = new InverterParametersData((byte)inverter.Index,
                                            GetShortInverterDescription(inverter.Type,
                                            inverter.IpAddress, inverter.TcpPort),
                                            fixedParameters);

            return inverterParametersWriteData;
        }

        #endregion
    }
}
