using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Newtonsoft.Json;

namespace Ferretto.VW.App
{
    public static class ParametersModelExtensions
    {
        #region Fields

        private static readonly JsonSerializerSettings jsonSettings;

        #endregion

        #region Constructors

        static ParametersModelExtensions()
        {
            jsonSettings = new JsonSerializerSettings();
            jsonSettings.ContractResolver = new CommonUtils.ContractResolver.FirstLetterPropertyNameResolver();
            jsonSettings.Converters.Add(new CommonUtils.Converters.IPAddressConverter());
            jsonSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }

        #endregion

        #region Methods

        public static T DeepClone<T>(this T input)
                    where T : class
                    => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(input ?? throw new ArgumentNullException(nameof(input)), jsonSettings), jsonSettings);

        /// <summary>
        /// Returns whether a <paramref name="configuration"/> includes the machine parameters.
        /// </summary>
        /// <remarks>It is intended as a centralized way to assert a fact that might be opinionated or amendable.</remarks>
        public static bool HasParameters(this VertimagConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (configuration.Machine == null)
            {
                return false;
            }

            return configuration.Machine.Elevator != null;
        }

        public static T ParseJson<T>(this FileInfo file)
            => JsonConvert.DeserializeObject<T>(
                File.ReadAllText(file?.FullName ?? throw new ArgumentNullException(nameof(file))),
                jsonSettings);

        #endregion
    }
}
