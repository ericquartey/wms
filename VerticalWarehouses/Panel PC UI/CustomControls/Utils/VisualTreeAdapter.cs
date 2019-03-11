using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Ferretto.VW.CustomControls
{
    public class VisualTreeAdapter : ILinqTree<DependencyObject>
    {
        #region Fields

        private readonly DependencyObject item;

        #endregion Fields

        #region Constructors

        public VisualTreeAdapter(DependencyObject item)
        {
            this.item = item;
        }

        #endregion Constructors

        #region Properties

        public DependencyObject Parent
        {
            get { return VisualTreeHelper.GetParent(this.item); }
        }

        #endregion Properties

        #region Methods

        public IEnumerable<DependencyObject> Children()
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(this.item);
            for (int i = 0; i < childrenCount; i++)
            {
                yield return VisualTreeHelper.GetChild(this.item, i);
            }
        }

        #endregion Methods
    }
}
