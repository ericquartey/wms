namespace Ferretto.VW.App.Accessories.Interfaces
{
    public class RegexMatchEventArgs : System.EventArgs
    {
        #region Constructors

        public RegexMatchEventArgs(string token, int index)
        {
            this.Token = token;
            this.Index = index;
        }

        #endregion

        #region Properties

        public int Index { get; }

        public string Token { get; }

        #endregion
    }
}
