using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Exceptions;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class BaysConfigurationProvider : Interfaces.IBaysConfigurationProvider
    {
        #region Fields

        private readonly Interfaces.IBaysProvider baysProvider;

        private readonly IGeneralInfoConfigurationDataLayer generalInfoConfiguration;

        private readonly ISetupNetworkDataLayer networkConfiguration;

        #endregion

        #region Constructors

        public BaysConfigurationProvider(
            IGeneralInfoConfigurationDataLayer generalInfoConfiguration,
            ISetupNetworkDataLayer networkConfiguration,
            Interfaces.IBaysProvider baysProvider)
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

        public void LoadFromConfiguration()
        {
            var baysCount = this.generalInfoConfiguration.BaysQuantity;
            var ipAddresses = new List<System.Net.IPAddress>();
            var bayTypes = new List<BayType>();

            switch (baysCount)
            {
                case 1:
                    ipAddresses.Add(this.networkConfiguration.PPC1MasterIPAddress);

                    bayTypes.Add((BayType)this.generalInfoConfiguration.Bay1Type);
                    break;

                case 2:
                    ipAddresses.Add(this.networkConfiguration.PPC1MasterIPAddress);
                    ipAddresses.Add(this.networkConfiguration.PPC2SlaveIPAddress);

                    bayTypes.Add((BayType)this.generalInfoConfiguration.Bay1Type);
                    bayTypes.Add((BayType)this.generalInfoConfiguration.Bay2Type);

                    break;

                case 3:
                    ipAddresses.Add(this.networkConfiguration.PPC1MasterIPAddress);
                    ipAddresses.Add(this.networkConfiguration.PPC2SlaveIPAddress);
                    ipAddresses.Add(this.networkConfiguration.PPC3SlaveIPAddress);

                    bayTypes.Add((BayType)this.generalInfoConfiguration.Bay1Type);
                    bayTypes.Add((BayType)this.generalInfoConfiguration.Bay2Type);
                    bayTypes.Add((BayType)this.generalInfoConfiguration.Bay3Type);

                    break;

                default:
                    throw new Exception($"Unexpected amount of bays in configuration ({baysCount}).");
            }

            for (var i = 0; i < baysCount; i++)
            {
                var bayNumber = i + 1;
                try
                {
                    var bay = this.baysProvider.GetByNumber(bayNumber);

                    this.baysProvider.Update(
                        bayNumber,
                        ipAddresses[i].ToString(),
                        bayTypes[i]);
                }
                catch (EntityNotFoundException)
                {
                    this.baysProvider.Create(new Bay
                    {
                        Number = bayNumber,
                        ExternalId = bayNumber,
                        IpAddress = ipAddresses[i].ToString(),
                        Type = bayTypes[i],
                    });
                }
            }
        }

        #endregion
    }
}
