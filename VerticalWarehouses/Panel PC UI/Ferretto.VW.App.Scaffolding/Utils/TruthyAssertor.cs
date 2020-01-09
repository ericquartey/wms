namespace Ferretto.VW.App.Scaffolding
{
    public static class TruthyAssertor
    {
        public static bool IsTruthy(object value)
        {
            if (value is System.Collections.IEnumerable enumerable)
            {
                return enumerable?.GetEnumerator().MoveNext() == true;
            }

            if (value is string text)
            {
                return !string.IsNullOrEmpty(text);
            }

            if (value is int count)
            {
                return count != default;
            }

            if (value is bool boolean)
            {
                return boolean;
            }

            return value != null;
        }
    }
}
