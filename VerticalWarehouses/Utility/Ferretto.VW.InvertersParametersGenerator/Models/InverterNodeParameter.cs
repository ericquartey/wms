namespace Ferretto.VW.InvertersParametersGenerator.Models
{
    public class InverterNodeParameter
    {
        #region Constructors

        public InverterNodeParameter(short code, string description, string value)
        {
            this.Code = code;
            this.Description = description;
            this.Value = value;
        }

        #endregion

        #region Properties

        public short Code { get; set; }

        public string Description { get; set; }

        public string Value { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Code {this.Code}, Description '{this.Description}', value {this.Value}";
        }

        #endregion
    }
}
