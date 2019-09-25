using System;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal sealed class BaysConfigurationProvider : Interfaces.IBaysConfigurationProvider
    {
        #region Fields

        private readonly Interfaces.IBaysProvider baysProvider;

        #endregion

        #region Constructors

        public BaysConfigurationProvider(
            Interfaces.IBaysProvider baysProvider)
        {
            if (baysProvider is null)
            {
                throw new ArgumentNullException(nameof(baysProvider));
            }

            this.baysProvider = baysProvider;
        }

        #endregion

        #region Methods

        public void LoadFromConfiguration()
        {
            this.baysProvider.AddElevatorPseudoBay();
        }

        #endregion
    }
}
