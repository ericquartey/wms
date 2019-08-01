using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Utils
{
    public class DataGridMaintenanceDetail : BindableBase
    {
        #region Fields

        private string description;

        private string element;

        private string quantity;

        #endregion

        #region Properties

        public string Description { get => this.description; set => this.SetProperty(ref this.description, value); }

        public string Element { get => this.element; set => this.SetProperty(ref this.element, value); }

        public string Quantity { get => this.quantity; set => this.SetProperty(ref this.quantity, value); }

        #endregion
    }
}
