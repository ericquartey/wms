using System.ComponentModel;

namespace Ferretto.Common.Utils.Expressions
{
    public class SortOption
    {
        #region Constructors

        public SortOption(string propertyName, ListSortDirection direction)
        {
            this.PropertyName = propertyName;
            this.Direction = direction;
        }

        #endregion Constructors

        #region Properties

        public ListSortDirection Direction { get; private set; }

        public string PropertyName { get; private set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return $"{this.PropertyName} {this.Direction}";
        }

        #endregion Methods
    }
}
