using DevExpress.Mvvm;
using Ferretto.WMS.App.Controls.Services;

namespace Ferretto.WMS.App.Controls
{
    public class Notification : BindableBase
    {
        #region Fields

        private StatusType mode;

        #endregion

        #region Properties

        public int Id { get; set; }

        public string ImgUrl { get; set; }

        public string Message { get; set; }

        public StatusType Mode
        {
            get => this.mode;
            set
            {
                this.mode = value;
                this.RaisePropertiesChanged(nameof(this.Mode));
            }
        }

        public string Title { get; set; }

        #endregion
    }
}
