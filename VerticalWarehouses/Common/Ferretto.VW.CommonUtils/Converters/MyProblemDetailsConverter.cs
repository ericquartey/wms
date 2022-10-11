using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Ferretto.VW.CommonUtils.Converters
{
    public sealed class MyProblemDetailsConverter : JsonConverter
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

            var problemDetails = new ProblemDetails();
            IList<PropertyInfo> props = new List<PropertyInfo>(value.GetType().GetProperties());
            foreach (PropertyInfo property in props)
            {
                object propValue = property.GetValue(value, null);
                if (propValue != null)
                {
                    switch (property.Name)
                    {
                        case "Detail":
                            problemDetails.Detail = propValue?.ToString();
                            break;

                        case "Type":
                            problemDetails.Type = propValue?.ToString();
                            break;

                        case "Title":
                            problemDetails.Title = propValue?.ToString();
                            break;

                        case "Status":
                            problemDetails.Status = (int?)propValue;
                            break;

                        case "Instance":
                            problemDetails.Instance = propValue?.ToString();
                            break;

                        case "Extensions":
                            var extensions = (IDictionary<string, object>)propValue;
                            foreach (var extension in extensions)
                            {
                                problemDetails.Extensions.Add(extension);
                            }
                            break;
                    }
                }
            }
            var annotatedProblemDetails = new ProblemDetailsHelper(problemDetails);

            serializer.Serialize(writer, annotatedProblemDetails);
        }

        #endregion
    }
}
