using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ferretto.VW.App
{
    public static class ParametersModelExtensions
    {
        #region Fields

        private const string CONFIGURATION_FILENAME = "vertimag-configuration.{0}-{1}{2}.json";

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

        public static string Filename(this VertimagConfiguration source, DriveInfo drive, bool unique)
        {
            var serial = source?.Machine?.SerialNumber;
            if (string.IsNullOrEmpty(nameof(serial)))
            {
                throw new ArgumentException("Cannot retrieve a serial code from the configuration.", nameof(source));
            }
            var name = Regex.Replace(serial, @"[^\w\.-]", string.Empty);
            string tick = default, filename = default;
            var incremental = 0;

            do
            {
                filename = System.IO.Path.Combine(
                  (drive ?? throw new ArgumentNullException(nameof(drive))).RootDirectory.FullName,
                  string.Format(System.Globalization.CultureInfo.InvariantCulture, CONFIGURATION_FILENAME, name, AssemblyInfo.Version, tick));

                incremental++;
                tick = string.Concat("(", incremental, ")");
            } while (unique && File.Exists(filename));

            return filename;
        }

        public static string JsonPropertyName(this PropertyInfo prop)
         => (prop ?? throw new ArgumentNullException(nameof(prop))).GetCustomAttributes<JsonPropertyAttribute>().Select(p => p.PropertyName).FirstOrDefault() ?? prop.Name;

        public static JObject ExtendWith(this object target, params object[] sources)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new Ferretto.VW.CommonUtils.Converters.IPAddressConverter());
            var retval = JObject.FromObject(target, serializer);

            if (sources != null)
            {
                foreach (var source in sources)
                {
                    retval.Merge(JObject.FromObject(source, serializer), new JsonMergeSettings
                    {
                        MergeNullValueHandling = MergeNullValueHandling.Ignore,
                        MergeArrayHandling = MergeArrayHandling.Replace,
                        PropertyNameComparison = StringComparison.Ordinal,
                    });
                }
            }

            return retval;
        }

        #endregion
    }
}
