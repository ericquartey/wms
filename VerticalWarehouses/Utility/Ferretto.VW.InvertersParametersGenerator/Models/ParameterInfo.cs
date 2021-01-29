using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.InvertersParametersGenerator.Models
{
    public class ParameterInfo
    {
        #region Constructors

        public ParameterInfo(short code, string description, string type, string um, bool isReadOnly)
        {
            this.Code = code;
            this.Type = type;
            this.IsReadOnly = isReadOnly;
            this.Description = description;
            this.Um = um;
        }

        #endregion Constructors

        #region Properties

        public short Code { get; }

        public bool IsReadOnly { get; }

        public string Type { get; }

        public string Description { get; }

        public string Um { get; }

        #endregion Properties
    }
}
