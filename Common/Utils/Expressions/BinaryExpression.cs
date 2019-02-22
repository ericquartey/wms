namespace Ferretto.Common.Utils.Expressions
{
    public sealed class BinaryExpression : IExpression
    {
        #region Constructors

        public BinaryExpression(string operatorName)
        {
            this.OperatorName = operatorName;
        }

        #endregion

        #region Properties

        public string Format => this.OperatorName + "({0},{1})";

        public IExpression LeftExpression { get; set; }

        public string OperatorName { get; }

        public IExpression RightExpression { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return string.Format(this.Format, this.LeftExpression, this.RightExpression);
        }

        #endregion
    }
}
