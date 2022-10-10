using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Ferretto.VW.CommonUtils.Converters
{
    internal class ProblemDetailsHelper
    {
        #region Constructors

        public ProblemDetailsHelper()
        { }

        public ProblemDetailsHelper(ProblemDetails problemDetails)
        {
            this.Detail = problemDetails.Detail;
            this.Instance = problemDetails.Instance;
            this.Status = problemDetails.Status;
            this.Title = problemDetails.Title;
            this.Type = problemDetails.Type;

            foreach (var kvp in problemDetails.Extensions)
            {
                this.Extensions[kvp.Key] = kvp.Value;
            }
        }

        #endregion

        #region Properties

        [JsonProperty(PropertyName = "detail", NullValueHandling = NullValueHandling.Ignore)]
        public string Detail { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> Extensions { get; } = new Dictionary<string, object>(StringComparer.Ordinal);

        [JsonProperty(PropertyName = "instance", NullValueHandling = NullValueHandling.Ignore)]
        public string Instance { get; set; }

        [JsonProperty(PropertyName = "status", NullValueHandling = NullValueHandling.Ignore)]
        public int? Status { get; set; }

        [JsonProperty(PropertyName = "title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        #endregion

        #region Methods

        public void CopyTo(ProblemDetails problemDetails)
        {
            problemDetails.Type = this.Type;
            problemDetails.Title = this.Title;
            problemDetails.Status = this.Status;
            problemDetails.Instance = this.Instance;
            problemDetails.Detail = this.Detail;

            foreach (var kvp in this.Extensions)
            {
                problemDetails.Extensions[kvp.Key] = kvp.Value;
            }
        }

        #endregion
    }
}
