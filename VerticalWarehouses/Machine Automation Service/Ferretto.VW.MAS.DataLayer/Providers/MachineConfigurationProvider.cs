using System;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    public class MachineConfigurationProvider : IMachineConfigurationProvider
    {
        #region Fields

        private const int MaxDrawerGrossWeight = 990;

        private readonly IGeneralInfoConfigurationDataLayer generalInfo;

        #endregion

        #region Constructors

        public MachineConfigurationProvider(IGeneralInfoConfigurationDataLayer generalInfo)
        {
            this.generalInfo = generalInfo ?? throw new ArgumentNullException(nameof(generalInfo));
        }

        #endregion



        #region Methods

        public bool IsOneKMachine()
        {
            return this.generalInfo.MaxDrawerGrossWeight == MaxDrawerGrossWeight;
        }

        #endregion
    }
}
