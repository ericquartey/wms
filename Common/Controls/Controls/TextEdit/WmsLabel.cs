using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    public class WmsLabel : Label
    {
        #region Fields

        public static readonly DependencyProperty TitleProperty = DependencyProperty.RegisterAttached(
           nameof(Title), typeof(string), typeof(WmsLabel), new UIPropertyMetadata(OnTitleChanged));

        #endregion

        #region Properties

        public string Title { get => (string)this.GetValue(TitleProperty); set => this.SetValue(TitleProperty, value); }

        #endregion

        #region Methods

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion
    }
}
