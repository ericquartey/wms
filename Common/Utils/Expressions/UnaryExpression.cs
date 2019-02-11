namespace Ferretto.Common.Utils.Expressions
{
    public class UnaryExpression : IExpression
    {
        #region Constructors

        public UnaryExpression(string operatorName)
        {
            this.Format = $"{operatorName}({{0}})";
        }

        #endregion

        #region Properties

        public IExpression Expression { get; set; }

        public string Format { get; private set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return string.Format(this.Format, this.Expression?.ToString());
        }

        #endregion
    }
}
