using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Exceptions;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IVertimagConfiguration
    {
        #region Methods

        public async Task<Dictionary<InverterIndex, InverterType>> GetInstalledInverterListAsync()
        {
            const int MAX_INVERTER_NUMBER = 8;
            long setupNetworkInverterIndex;
            InverterType inverterType;

            var installedInverters = new Dictionary<InverterIndex, InverterType>
            {
                { InverterIndex.MainInverter, InverterType.Ang }
            };

            for (var i = 1; i < MAX_INVERTER_NUMBER; i++)
            {
                switch (i)
                {
                    case 1:
                        setupNetworkInverterIndex = (long)SetupNetwork.InverterIndexChain;
                        inverterType = InverterType.Ang;
                        break;

                    case 2:
                        setupNetworkInverterIndex = (long)SetupNetwork.InverterIndexShutter1;
                        inverterType = InverterType.Agl;
                        break;

                    case 3:
                        setupNetworkInverterIndex = (long)SetupNetwork.InverterIndexShutter2;
                        inverterType = InverterType.Agl;
                        break;

                    case 4:
                        setupNetworkInverterIndex = (long)SetupNetwork.InverterIndexShutter3;
                        inverterType = InverterType.Agl;
                        break;

                    case 5:
                        setupNetworkInverterIndex = (long)SetupNetwork.InverterIndexBay1;
                        inverterType = InverterType.Acu;
                        break;

                    case 6:
                        setupNetworkInverterIndex = (long)SetupNetwork.InverterIndexBay2;
                        inverterType = InverterType.Acu;
                        break;

                    case 7:
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
                    setupNetworkInverterIndex = await this.GetIntegerConfigurationValueAsync(setupNetworkInverterIndex, (long)ConfigurationCategory.SetupNetwork);
                    Enum.TryParse(setupNetworkInverterIndex.ToString(), out InverterIndex inverterIndex);
                    installedInverters.TryAdd<InverterIndex, InverterType>(inverterIndex, inverterType);
                }
                catch (DataLayerPersistentException ex)
                {
                    this.logger.LogTrace($"SetUp Network parameter not found: {setupNetworkInverterIndex} - Message: {ex.Message}");
                }
                catch (Exception ex)
                {
                    this.logger.LogTrace($"{ex.Message}");
                }
            }

            await Task.Delay(5, this.stoppingToken);

            return installedInverters;
        }

        #endregion
    }
}
