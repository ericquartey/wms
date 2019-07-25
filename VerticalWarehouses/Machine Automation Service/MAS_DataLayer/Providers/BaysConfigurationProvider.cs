using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    internal class BaysConfigurationProvider : IBaysConfgurationProvider
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly IGeneralInfoConfigurationDataLayer generalInfoConfiguration;

        private readonly ISetupNetworkDataLayer networkConfiguration;

        #endregion

        #region Constructors

        public BaysConfigurationProvider(
            IGeneralInfoConfigurationDataLayer generalInfoConfiguration,
            ISetupNetworkDataLayer networkConfiguration,
            IBaysProvider baysProvider)
        {
            if (generalInfoConfiguration == null)
            {
                throw new ArgumentNullException(nameof(generalInfoConfiguration));
            }

            if (networkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(networkConfiguration));
            }

            if (baysProvider == null)
            {
                throw new ArgumentNullException(nameof(baysProvider));
            }

            this.generalInfoConfiguration = generalInfoConfiguration;
            this.networkConfiguration = networkConfiguration;
            this.baysProvider = baysProvider;
        }

        #endregion

        #region Methods

        public async Task LoadFromConfigurationAsync()
        {
            var baysCount = await this.generalInfoConfiguration.BaysQuantity;
            var ipAddresses = new List<System.Net.IPAddress>();
            var bayTypes = new List<BayType>();

            switch (baysCount)
            {
                case 1:
                    ipAddresses.Add(await this.networkConfiguration.PPC1MasterIPAddress);

                    bayTypes.Add((BayType)await this.generalInfoConfiguration.Bay1Type);
                    break;

                case 2:
                    ipAddresses.Add(await this.networkConfiguration.PPC1MasterIPAddress);
                    ipAddresses.Add(await this.networkConfiguration.PPC2SlaveIPAddress);

                    bayTypes.Add((BayType)await this.generalInfoConfiguration.Bay1Type);
                    bayTypes.Add((BayType)await this.generalInfoConfiguration.Bay2Type);

                    break;

                case 3:
                    ipAddresses.Add(await this.networkConfiguration.PPC1MasterIPAddress);
                    ipAddresses.Add(await this.networkConfiguration.PPC2SlaveIPAddress);
                    ipAddresses.Add(await this.networkConfiguration.PPC3SlaveIPAddress);

                    bayTypes.Add((BayType)await this.generalInfoConfiguration.Bay1Type);
                    bayTypes.Add((BayType)await this.generalInfoConfiguration.Bay2Type);
                    bayTypes.Add((BayType)await this.generalInfoConfiguration.Bay3Type);

                    break;

                default:
                    throw new Exception($"Unexpected amount of bays in configuration ({baysCount}).");
            }

            for (var i = 0; i < baysCount; i++)
            {
                var bay = this.baysProvider.GetById(i);
                if (bay == null)
                {
                    this.baysProvider.Create(new Bay
                    {
                        Id = i,
                        IpAddress = ipAddresses[i].ToString(),
                        Type = bayTypes[i],
                        Status = BayStatus.Unavailable,
                    });
                }
                else
                {
                    this.baysProvider.Update(
                        i,
                        ipAddresses[i].ToString(),
                        bayTypes[i]);
                }
            }
        }

        #endregion
    }
}
