using System;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class InverterParameter : DataModel
    {
        #region Constructors

        public InverterParameter()
        {
        }

        public InverterParameter(InverterParameter seed)
        {
            if (seed is null)
            {
                throw new ArgumentNullException(nameof(seed));
            }
            this.Id = seed.Id;
            this.Code = seed.Code;
            this.DataSet = seed.DataSet;
            this.DecimalCount = seed.DecimalCount;
            this.Description = seed.Description;
            this.Error = seed.Error;
            this.IsReadOnly = seed.IsReadOnly;
            this.ReadCode = seed.ReadCode;
            this.StringValue = seed.StringValue;
            this.Type = seed.Type;
            this.Um = seed.Um;
            this.WriteCode = seed.WriteCode;
        }

        #endregion

        #region Properties

        public short Code { get; set; }

        public int DataSet { get; set; }

        public int DecimalCount { get; set; }

        public string Description { get; set; }

        public bool Error { get; set; }

        public bool IsReadOnly { get; set; }

        public object Payload
        {
            get
            {
                try
                {
                    if (this.Type == "string")
                    {
                        return this.StringValue;
                    }
                    else if (this.Type == "short")
                    {
                        return short.Parse(this.StringValue);
                    }
                    else if (this.Type == "ushort")
                    {
                        return ushort.Parse(this.StringValue);
                    }
                    else if (this.Type == "int")
                    {
                        return int.Parse(this.StringValue);
                    }
                    else if (this.Type == null)
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"{ex.Message}: On parameter {this.Code}, Type {this.Type} for value '{this.StringValue}', maybe is null or empty ");
                }
                throw new ArgumentException($"Type {this.Type} for value {this.StringValue}, not supported on parameter {this.Code}");
            }
        }

        public short ReadCode { get; set; }

        public string StringValue { get; set; }

        public string Type { get; set; }

        public string Um { get; set; }

        public short WriteCode { get; set; }

        #endregion
    }
}
