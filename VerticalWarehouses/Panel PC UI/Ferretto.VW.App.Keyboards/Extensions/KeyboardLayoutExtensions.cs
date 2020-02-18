using System;
using System.Collections.Generic;
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
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            if (keyboard == null)
            {
                return;
            }

            grid.HorizontalAlignment = keyboard.HorizontalAlignment;

            // ===========================================================================
            // define the grid
            var zones = new Dictionary<string, KeyboardZone>();
            foreach (var zone in keyboard.Zones)
            {
                // dict (let it throw when keys are duplicated)
                zones.Add(zone.Id, zone);

                // ensure columns and rows
                while (grid.RowDefinitions.Count < (zone.Row + zone.RowSpan))
                {
                    grid.RowDefinitions.Add(new RowDefinition
                    {
                        Height = GridLength.Auto
                    });
                }
                while (grid.ColumnDefinitions.Count < (zone.Column + zone.ColumnSpan))
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition
                    {
                        Width = new GridLength(1, GridUnitType.Star)
                    });
                }

                // specify the correct width, if any.
                if (zone.ColumnSpan == 1 && zone.Width.HasValue)
                {
                    grid.ColumnDefinitions[zone.Column].Width = zone.Width.Value;
                }
            }

            // ===========================================================================
            // fill the grid
            foreach (var set in keyboard.Sets)
            {
                Thickness? keyMargin = set.KeyMargin ?? keyboard.KeyMargin,
                    keyPadding = set.KeyPadding ?? keyboard.KeyPadding;

                double? rowKeyMinWidth = set.KeyMinWidth ?? keyboard.KeyMinWidth;

                string rowStyleResource = set.KeyStyleResource ?? keyboard.KeyStyleResource;

                KeyboardCell[] cells = set.Cells?.ToArray() ?? Array.Empty<KeyboardCell>();
                int cellCount = cells.Length;

                var setGrid = new Grid
                {
                    HorizontalAlignment = set.HorizontalAlignment,
                    VerticalAlignment = set.VerticalAlignment,
                };

                // pick the relevant zone (let it throw if key is empty or there is no match)
                var zone = zones[set.Zone];
                Grid.SetColumn(setGrid, zone.Column);
                Grid.SetColumnSpan(setGrid, zone.ColumnSpan);
                Grid.SetRow(setGrid, zone.Row);
                Grid.SetRowSpan(setGrid, zone.RowSpan);

                grid.Children.Add(setGrid);

                for (int j = 0; j < cellCount; j++)
                {
                    var cell = cells[j];
                    setGrid.ColumnDefinitions.Add(new ColumnDefinition
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
                        setGrid.Children.Add(btn);
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
                        setGrid.Children.Add(brd);
                    }
                }
            }
        }

        #endregion
    }
}
