using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CollapsibleRow : RowDefinition
    {
        #region Fields

        public static readonly DependencyProperty IsVisibleProperty;

        #endregion

        #region Constructors

        static CollapsibleRow()
        {
            IsVisibleProperty = DependencyProperty.Register(nameof(IsVisible),
                typeof(bool), typeof(CollapsibleRow), new PropertyMetadata(false, OnCollapsedChanged));

            RowDefinition.HeightProperty.OverrideMetadata(typeof(CollapsibleRow),
                new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star), null, CoerceHeight));

            RowDefinition.MinHeightProperty.OverrideMetadata(typeof(CollapsibleRow),
                new FrameworkPropertyMetadata(0.0, null, CoerceHeight));

            RowDefinition.MaxHeightProperty.OverrideMetadata(typeof(CollapsibleRow),
                new FrameworkPropertyMetadata(double.PositiveInfinity, null, CoerceHeight));
        }

        #endregion

        #region Properties

        public bool IsVisible
        {
            get => (bool)this.GetValue(IsVisibleProperty);
            set => this.SetValue(IsVisibleProperty, value);
        }

        #endregion

        #region Methods

        private static object CoerceHeight(DependencyObject d, object baseValue)
        {
            return !(((CollapsibleRow)d).IsVisible) ? (baseValue is GridLength ? new GridLength(0) : 0.0 as object) : baseValue;
        }

        private static void OnCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(RowDefinition.HeightProperty);
            d.CoerceValue(RowDefinition.MinHeightProperty);
            d.CoerceValue(RowDefinition.MaxHeightProperty);
        }

        #endregion
    }
}
