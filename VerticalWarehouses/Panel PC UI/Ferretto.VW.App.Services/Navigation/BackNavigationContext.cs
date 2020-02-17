namespace Ferretto.VW.App.Services
{
    public class BackNavigationContext
    {
        #region Constructors

        public BackNavigationContext()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets whether this navigation process has to be prevented.
        /// </summary>
        public bool Cancel
        {
            get;
            set;
        }

        #endregion
    }
}
