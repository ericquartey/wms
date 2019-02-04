namespace Ferretto.Common.Controls.Services
{
    public class MouseDownInfo
    {
        #region Constructors

        public MouseDownInfo(object sender, object originalSource, MouseButtonPressed buttonPressed, object uiElementObj, object data)
        {
            this.Sender = sender;
            this.OriginalSource = originalSource;
            this.ButtonPressed = buttonPressed;
            this.UIElementObj = uiElementObj;
            this.Data = data;
        }

        #endregion

        #region Properties

        public MouseButtonPressed ButtonPressed { get; set; }

        public object Data { get; set; }

        public bool IsHandled { get; set; }

        public object OriginalSource { get; set; }

        public object Sender { get; set; }

        public object UIElementObj { get; set; }

        #endregion
    }
}
