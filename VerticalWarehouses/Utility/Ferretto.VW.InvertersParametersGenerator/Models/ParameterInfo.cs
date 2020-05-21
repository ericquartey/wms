using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.InvertersParametersGenerator.Models
{
    public class ParameterInfo
    {
        #region Constructors

        public ParameterInfo(short code, string description, string type, bool isReadOnly)
        {
            this.Code = code;
            this.Description = description;
            this.Type = type;
            this.IsReadOnly = isReadOnly;
        }

        #endregion

        #region Properties

        public short Code { get; }

        public string Description { get; }

        public bool IsReadOnly { get; }

        public string Type { get; }

        #endregion
    }
}
