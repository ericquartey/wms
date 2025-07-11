﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ferretto.VW.App.Controls.Extensions
{
    public class DataGridExtensions
    {
        #region Fields

        public static readonly DependencyProperty ScrollChangedCommandProperty = DependencyProperty.RegisterAttached(
            "ScrollChangedCommand", typeof(ICommand), typeof(DataGridExtensions),
            new PropertyMetadata(default(ICommand), OnScrollChangedCommandChanged));

        #endregion

        #region Methods

        public static ICommand GetScrollChangedCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(ScrollChangedCommandProperty);
        }

        public static void SetScrollChangedCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(ScrollChangedCommandProperty, value);
        }

        private static void DataGridOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
            {
                return;
            }

            var scrollViewer = UIHelper.FindChildren<ScrollViewer>(dataGrid).FirstOrDefault();
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += ScrollViewerOnScrollChanged;
            }
        }

        private static void OnScrollChangedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = d as DataGrid;
            if (dataGrid == null)
            {
                return;
            }

            if (e.NewValue != null)
            {
                dataGrid.Loaded += DataGridOnLoaded;
            }
            else if (e.OldValue != null)
            {
                dataGrid.Loaded -= DataGridOnLoaded;
            }
        }

        private static void ScrollViewerOnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var dataGrid = UIHelper.FindParent<DataGrid>(sender as ScrollViewer);
            if (dataGrid != null)
            {
                var command = GetScrollChangedCommand(dataGrid);
                command.Execute(e);
            }
        }

        #endregion
    }
}
