using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

        public IEnumerable<KeyboardRow> Rows { get; set; } = Array.Empty<KeyboardRow>();

        #endregion

        #region Methods

        public static KeyboardLayout FromJson(string json)
            => JsonConvert.DeserializeObject<KeyboardLayout>(json, _jsonSettings);

        public string ToJson()
            => JsonConvert.SerializeObject(this, _jsonSettings);

        #endregion
    }
}
