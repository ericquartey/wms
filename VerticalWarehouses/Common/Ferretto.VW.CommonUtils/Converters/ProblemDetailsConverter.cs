using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Ferretto.VW.CommonUtils.Converters
{
    public sealed class ProblemDetailsConverter : JsonConverter
    {
        #region Methods

        public override bool CanConvert(Type objectType)
        {
            return objectType.Name == "AnnotatedProblemDetails";
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.Value is null)
            {
                return null;
            }
            var annotatedProblemDetails = serializer.Deserialize<ProblemDetailsHelper>(reader);
            if (annotatedProblemDetails == null)
            {
                return null;
            }

            var problemDetails = (ProblemDetails)existingValue ?? new ProblemDetails();
            annotatedProblemDetails.CopyTo(problemDetails);
            return problemDetails;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var problemDetails = (ProblemDetails)value;
            var annotatedProblemDetails = new ProblemDetailsHelper(problemDetails);

            serializer.Serialize(writer, annotatedProblemDetails);
        }

        #endregion
    }
}
