using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.InverterDriver
{
    public static class ConfigurationExtensions
    {
        #region Fields

        private const string VertimagInverterDriverUseMockKey = "Vertimag:Drivers:Inverter:UseMock";

        #endregion

        #region Methods

        public static bool UseInverterDriverMock(this IConfiguration configuration)
        {
            return configuration.GetValue<bool>(VertimagInverterDriverUseMockKey);
        }

        #endregion
    }
}
