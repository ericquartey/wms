namespace Ferretto.Common.Controls.Services
{
    public class ShortKeyAction
    {
        #region Constructors

        public ShortKeyAction(object viewModel, object element, ShortKey shortKey)
        {
            this.ViewModel = viewModel;
            this.Element = element;
            this.ShortKey = shortKey;
        }

        #endregion

        #region Properties

        public object Element { get; set; }

        public bool IsHandled { get; set; }

        public ShortKey ShortKey { get; set; }

        public object ViewModel { get; set; }

        #endregion
    }
}
