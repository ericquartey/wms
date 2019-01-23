using System.Collections;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    public partial class TileBar : ListBox
    {
        #region Constructors

        public TileBar()
        {
            this.InitializeComponent();

            this.Loaded += this.TileBar_Loaded;
        }

        #endregion Constructors

        #region Methods

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            this.SelectFirstItem();
        }

        private void SelectFirstItem()
        {
            this.SelectedItem = this.Items?.Count > 0 ? this.Items.GetItemAt(0) : null;
        }

        private void TileBar_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.SelectFirstItem();
        }

        #endregion Methods
    }
}
