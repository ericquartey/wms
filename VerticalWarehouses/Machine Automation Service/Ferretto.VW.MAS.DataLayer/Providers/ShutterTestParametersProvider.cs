using Ferretto.VW.MAS.DataLayer.DatabaseContext;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class ShutterTestParametersProvider : IShutterTestParametersProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public ShutterTestParametersProvider(DataLayerContext dataContext)
        {
            if (dataContext == null)
            {
                throw new System.ArgumentNullException(nameof(dataContext));
            }

            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public ShutterTestParameters Get()
        {
            return new ShutterTestParameters // TODO retrieve data from database
            {
                RequiredCycles = 200,
                DelayBetweenCycles = 5,
            };
        }

        #endregion
    }
}
