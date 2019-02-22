namespace Ferretto.Common.Utils.Expressions
{
    public class UnaryExpression : IExpression
    {
        #region Constructors

        public UnaryExpression(string operatorName)
        {
            this.OperatorName = operatorName;
        }

        #endregion

        #region Properties

        public string Format => this.OperatorName + "({0})";

        public IExpression Expression { get; set; }

        public string OperatorName { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return string.Format(this.Format, this.Expression);
        }

        #endregion
    }
}
