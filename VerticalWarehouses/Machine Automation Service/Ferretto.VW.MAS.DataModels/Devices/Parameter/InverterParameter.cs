using System;
using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class InverterParameter : DataModel
    {
        #region Properties

        public short Code { get; set; }

        public int DataSet { get; set; }

        public bool IsReadOnly { get; set; }

        public string StringValue { get; set; }

        public string Type { get; set; }

        public int Value { get; set; }

        #endregion
    }
}
