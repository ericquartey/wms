namespace Ferretto.WMS.Data.WebAPI.Models.Expressions
{
    public sealed class BinaryExpression : IExpression
    {
        #region Constructors

        public BinaryExpression(string operatorName)
        {
            this.OperatorName = operatorName;
        }

        #endregion Constructors

        #region Properties

        public string Format => this.OperatorName + "({0},{1})";

        public IExpression LeftExpression { get; set; }

        public string OperatorName { get; private set; }

        public IExpression RightExpression { get; set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return string.Format(this.Format, this.LeftExpression?.ToString(), this.RightExpression?.ToString());
        }

        #endregion Methods
    }
}
