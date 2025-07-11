﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Ferretto.VW.App.Controls.Utils
{
    public class VisualTreeAdapter : ILinqTree<DependencyObject>
    {
        #region Fields

        private readonly DependencyObject item;

        #endregion

        #region Constructors

        public VisualTreeAdapter(DependencyObject item)
        {
            this.item = item;
        }

        #endregion

        #region Properties

        public DependencyObject Parent => VisualTreeHelper.GetParent(this.item);

        #endregion

        #region Methods

        public IEnumerable<DependencyObject> Children()
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(this.item);
            for (var i = 0; i < childrenCount; i++)
            {
                yield return VisualTreeHelper.GetChild(this.item, i);
            }
        }

        #endregion
    }
}
