using System;

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
                //differtence type between inverter and c#
                try
                {
                    if (this.Type == "String")
                    {
                        return this.StringValue;
                    }
                    else if (this.Type == "Int")
                    {
                        return short.Parse(this.StringValue);
                    }
                    else if (this.Type == "uInt")
                    {
                        return ushort.Parse(this.StringValue);
                    }
                    else if (this.Type == "Long")
                    {
                        return int.Parse(this.StringValue);
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"{ex.Message}: On parameter {this.Code}, Type {this.Type} for value '{this.StringValue}', maybe is null or empty ");
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
