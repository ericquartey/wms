﻿using System;
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

        public object Payload
        {
            get
            {
                if (this.Type == "String")
                {
                    return this.StringValue;
                }
                else if (this.Type == "Int")
                {
                    return int.Parse(this.StringValue);
                }
                else if (this.Type == "uInt")
                {
                    return uint.Parse(this.StringValue);
                }
                else if (this.Type == "Long")
                {
                    return long.Parse(this.StringValue);
                }

                throw new ArgumentException($"Type {this.Type} for value {this.StringValue}, not supported on parameter {this.Code}");
            }
        }

        public string StringValue { get; set; }

        public string Type { get; set; }

        public int Value { get; set; }

        #endregion
    }
}
