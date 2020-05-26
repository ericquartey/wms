using System;
using System.Collections.Generic;
using System.Windows;
using Newtonsoft.Json;

namespace Ferretto.VW.App.Keyboards
{
    public class KeyboardLayout : KeyboardKeyContainer
    {
        #region Fields

        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        #endregion

        #region Constructors

        static KeyboardLayout()
        {
            _jsonSettings.Converters.Add(new Serialization.JsonGridLengthConverter());
        }

        #endregion

        #region Properties

        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;

        /// <summary>
        /// Gets or sets the zone definitions.
        /// </summary>
        public IEnumerable<KeyboardSet> Sets { get; set; } = Array.Empty<KeyboardSet>();

        /// <summary>
        /// Gets or sets the keysets.
        /// </summary>
        public IEnumerable<KeyboardZone> Zones { get; set; } = Array.Empty<KeyboardZone>();

        #endregion

        #region Methods

        public static KeyboardLayout FromJson(string json)
            => JsonConvert.DeserializeObject<KeyboardLayout>(json, _jsonSettings);

        public string ToJson()
            => JsonConvert.SerializeObject(this, _jsonSettings);

        #endregion
    }
}
