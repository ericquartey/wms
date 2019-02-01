namespace Ferretto.Common.Utils.Expressions
{
    public class ValueExpression : IExpression
    {
        #region Constructors

        public ValueExpression(string value)
        {
            this.Value = value;
        }

        #endregion Constructors

        #region Properties

        public string Format => "{0}";

        public string Value { get; private set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return string.Format(this.Format, this.Value);
        }

        #endregion Methods
    }
}
