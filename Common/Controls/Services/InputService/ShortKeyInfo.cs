namespace Ferretto.Common.Controls.Services
{
    public class ShortKeyInfo
    {
        #region Constructors

        public ShortKeyInfo(ShortKey shortKey, object uiElementObj, object data)
        {
            this.ShortKey = shortKey;
            this.UIElementObj = uiElementObj;
            this.Data = data;
        }

        #endregion Constructors

        #region Properties

        public object Data { get; set; }
        public ShortKey ShortKey { get; set; }

        public object UIElementObj { get; set; }

        #endregion Properties
    }
}
