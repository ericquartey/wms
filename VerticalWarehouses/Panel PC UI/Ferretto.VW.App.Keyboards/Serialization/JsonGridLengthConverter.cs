using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using Newtonsoft.Json;

namespace Ferretto.VW.App.Keyboards.Serialization
{
    public class JsonGridLengthConverter : JsonConverter<GridLength?>
    {
        #region Fields

        private readonly GridLengthConverter _converter = new GridLengthConverter();

        #endregion

        #region Methods

        public override GridLength? ReadJson(JsonReader reader, Type objectType, GridLength? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is string value)
            {
                return (GridLength)this._converter.ConvertFromString(value);
            }
            return default;
        }

        public override void WriteJson(JsonWriter writer, GridLength? value, JsonSerializer serializer)
        {
            if (value.HasValue)
            {
                string output = this._converter.ConvertToString(value.Value);
                writer.WriteValue(output);
            }
            else
            {
                writer.WriteNull();
            }
        }

        #endregion
    }
}
