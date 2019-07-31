using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Exceptions;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IVertimagConfigurationDataLayer
    {
        #region Methods

        public Dictionary<InverterIndex, InverterType> GetInstalledInverterList()
        {
            long setupNetworkInverterIndex;
            InverterType inverterType;

            var installedInverters = new Dictionary<InverterIndex, InverterType>();

            foreach (InverterIndex inverterIndex in Enum.GetValues(typeof(InverterIndex)))
            {
                switch (inverterIndex)
                {
                    case InverterIndex.MainInverter:
                        setupNetworkInverterIndex = (long)SetupNetwork.InverterIndexMaster;
                        inverterType = InverterType.Ang;
                        break;

                    case InverterIndex.Slave1:
                        setupNetworkInverterIndex = (long)SetupNetwork.InverterIndexChain;
                        inverterType = InverterType.Ang; //TEMP Verify
                        break;

                    case InverterIndex.Slave2:
                        setupNetworkInverterIndex = (long)SetupNetwork.InverterIndexShutter1;
                        inverterType = InverterType.Agl;
                        break;

                    case InverterIndex.Slave3:
                        setupNetworkInverterIndex = (long)SetupNetwork.InverterIndexBay1;
                        inverterType = InverterType.Acu;
                        break;

                    case InverterIndex.Slave4:
                        setupNetworkInverterIndex = (long)SetupNetwork.InverterIndexShutter2;
                        inverterType = InverterType.Agl;
                        break;

                    case InverterIndex.Slave5:
                        setupNetworkInverterIndex = (long)SetupNetwork.InverterIndexBay2;
                        inverterType = InverterType.Acu;
                        break;

                    case InverterIndex.Slave6:
                        setupNetworkInverterIndex = (long)SetupNetwork.InverterIndexShutter3;
                        inverterType = InverterType.Agl;
                        break;

                    case InverterIndex.Slave7:
                        setupNetworkInverterIndex = (long)SetupNetwork.InverterIndexBay3;
                        inverterType = InverterType.Acu;
                        break;

                    default:
                        setupNetworkInverterIndex = (long)SetupNetwork.Undefined;
                        inverterType = InverterType.Undefined;
                        break;
                }

                try
                {
                    setupNetworkInverterIndex = this.GetIntegerConfigurationValue(setupNetworkInverterIndex, ConfigurationCategory.SetupNetwork);
                    installedInverters.TryAdd<InverterIndex, InverterType>(inverterIndex, inverterType);
                }
                catch (DataLayerPersistentException ex)
                {
                    this.Logger.LogTrace($"SetUp Network parameter not found: {setupNetworkInverterIndex} - Message: {ex.Message}");
                }
                catch (Exception ex)
                {
                    this.Logger.LogTrace($"{ex.Message}");
                }
            }

            return installedInverters;
        }

        public List<IoIndex> GetInstalledIoList()
        {
            long setupNetworkIoIndex;
            var installedIoDevices = new List<IoIndex>();

            foreach (IoIndex ioIndex in Enum.GetValues(typeof(IoIndex)))
            {
                switch (ioIndex)
                {
                    case IoIndex.IoDevice1:
                        setupNetworkIoIndex = (long)SetupNetwork.IOExpansion1;
                        break;

                    case IoIndex.IoDevice2:
                        setupNetworkIoIndex = (long)SetupNetwork.IOExpansion2;
                        break;

                    case IoIndex.IoDevice3:
                        setupNetworkIoIndex = (long)SetupNetwork.IOExpansion3;
                        break;

                    default:
                        setupNetworkIoIndex = (long)SetupNetwork.Undefined;
                        break;
                }

                try
                {
                    var ipAddress = this.GetIpAddressConfigurationValue(setupNetworkIoIndex, ConfigurationCategory.SetupNetwork);
                    if (ipAddress != null)
                    {
                        installedIoDevices.Add(ioIndex);
                    }
                }
                catch (DataLayerPersistentException ex)
                {
                    this.Logger.LogTrace($"SetUp Network parameter not found: {setupNetworkIoIndex} - Message: {ex.Message}");
                }
                catch (Exception ex)
                {
                    this.Logger.LogTrace($"{ex.Message}");
                }
            }

            return installedIoDevices;
        }

        #endregion
    }
}
