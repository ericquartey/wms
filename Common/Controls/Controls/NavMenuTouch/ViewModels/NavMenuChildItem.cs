using System.Collections.ObjectModel;

namespace Ferretto.Common.Controls
{
    public class TileNavMenuChildItem
    {
        #region Constructors

        public TileNavMenuChildItem(string parentInfo)
        {
            this.Info = parentInfo;
            this.Children = new ObservableCollection<NavMenuItem>();
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<NavMenuItem> Children { get; set; }

        public string Info { get; set; }

        #endregion Properties
    }
}
