using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.App.Keyboards.Controls;

namespace Ferretto.VW.App.Keyboards
{
    public static class KeyboardLayoutExtensions
    {
        #region Methods

        public static void GenerateKeyboard(this Grid grid, KeyboardLayout keyboard)
        {
            (grid ?? throw new ArgumentNullException(nameof(grid))).Children.Clear();
            if (keyboard == null)
            {
                return;
            }

            int rowIndex = 0;
            foreach (var row in keyboard.Rows)
            {
                Thickness? keyMargin = row.KeyMargin ?? keyboard.KeyMargin,
                    keyPadding = row.KeyPadding ?? keyboard.KeyPadding;

                double? rowKeyMinWidth = row.KeyMinWidth ?? keyboard.KeyMinWidth;

                string rowStyleResource = row.KeyStyleResource ?? keyboard.KeyStyleResource;

                KeyboardCell[] cells = row.Cells?.ToArray() ?? Array.Empty<KeyboardCell>();
                int cellCount = cells.Length;
                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = row.Height
                });

                var rowGrid = new Grid
                {
                    HorizontalAlignment = row.HorizontalAlignment,
                    VerticalAlignment = row.VerticalAlignment,
                };
                Grid.SetRow(rowGrid, rowIndex++);
                grid.Children.Add(rowGrid);

                for (int j = 0; j < cellCount; j++)
                {
                    var cell = cells[j];
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition
                    {
                        Width = cell.Width ?? new GridLength(1, GridUnitType.Star)
                    });
                    var key = cell.Key;

                    Thickness padding = cell.KeyPadding ?? keyPadding ?? default,
                        margin = cell.KeyMargin ?? keyMargin ?? default;

                    string styleResource = cell.KeyStyleResource ?? rowStyleResource;

                    double minWidth = cell.KeyMinWidth ?? rowKeyMinWidth ?? default;
                    double minHeight = cell.KeyMinHeight ?? default;

                    if (key != null)
                    {
                        KeyboardButton btn = new KeyboardButton
                        {
                            Padding = padding,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Margin = margin,
                            Key = key,
                            MinWidth = minWidth,
                            MinHeight = minHeight
                        };
                        Grid.SetColumn(btn, j);
                        rowGrid.Children.Add(btn);
                        if (!string.IsNullOrEmpty(styleResource))
                        {
                            btn.SetResourceReference(KeyboardButton.KeyButtonStyleProperty, styleResource);
                        }
                    }
                    else
                    {
                        Border brd = new Border
                        {
                            Padding = padding,
                            Margin = margin,
                            MinWidth = minWidth,
                        };
                        Grid.SetColumn(brd, j);
                        rowGrid.Children.Add(brd);
                    }
                }
            }
        }

        #endregion
    }
}
