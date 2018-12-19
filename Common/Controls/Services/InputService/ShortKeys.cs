using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm.UI;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Utils;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls.Services
{
    public static class ShortKeys
    {
        #region Fields

        private static readonly List<ShortKey> MainKeys = new List<ShortKey>();
        private static readonly Dictionary<string, List<ShortKey>> ViewShortKeys = new Dictionary<string, List<ShortKey>>();

        #endregion Fields

        #region Methods

        public static ShortKey GetShortKey(Type view, ShortKey shortKey, out bool isMain)
        {
            ShortKey shortKeyFound = null;
            isMain = false;

            if (view != null)
            {
                // Check on view
                if (ViewShortKeys.ContainsKey(view.ToString()))
                {
                    var keys = ViewShortKeys[view.ToString()];
                    if (keys.Count > 0)
                    {
                        shortKeyFound = Getkey(keys, shortKey);
                    }
                }
            }

            // Check on main menu
            if (shortKeyFound == null)
            {
                shortKeyFound = Getkey(MainKeys, shortKey);
                isMain = (shortKeyFound != null);
            }
            return shortKeyFound;
        }

        public static void Initialize()
        {
            // MAIN SHORTKEYS
            // ******* Main Menu *********
            MainKeys.Add(new ShortKey(Key.I, true, ModifierKeys.Control, (v) =>
            {
                ServiceLocator.Current.GetInstance<INavigationService>().Appear(nameof(Utils.Modules.MasterData), Utils.Modules.MasterData.ITEMS);
            }));
            MainKeys.Add(new ShortKey(Key.C, true, ModifierKeys.Control, (v) =>
            {
                ServiceLocator.Current.GetInstance<INavigationService>().Appear(nameof(Utils.Modules.MasterData), Utils.Modules.MasterData.COMPARTMENTS);
            }));
            // **************************
            // ENDMAIN

            #region ******* LOGINVIEW

            var logiView = new List<ShortKey>();
            logiView.Add(new ShortKey(Key.Enter, false, (v) => {
                if (v.ShortKey.Key == Key.Enter &&
                    LayoutTreeHelper.GetVisualParents(v.Element as UIElement).OfType<INavigableView>().FirstOrDefault() is WmsDialogView view &&
                    LayoutTreeHelper.GetVisualChildren(view).OfType<ActionBar>().FirstOrDefault() is ActionBar actionBar)
                {
                    actionBar.Focus();
                }
            }));
            ViewShortKeys.Add(MvvmNaming.GetViewModelName(nameof(Utils.Modules.Layout), Utils.Modules.Layout.LOGINVIEW), logiView);

            #endregion ******* LOGINVIEW

            #region ******* ITEMDETAILS

            var itemDetails = new List<ShortKey>();
            itemDetails.Add(new ShortKey(Key.S, false, ModifierKeys.Control, (v) => { ((IEdit)v.ViewModel).SaveCommand.Execute(null); }));
            itemDetails.Add(new ShortKey(Key.R, false, ModifierKeys.Control, (v) => { ((IEdit)v.ViewModel).RevertCommand.Execute(null); }));
            ViewShortKeys.Add(MvvmNaming.GetViewModelName(nameof(Utils.Modules.MasterData), Utils.Modules.MasterData.ITEMDETAILS), itemDetails);

            #endregion ******* ITEMDETAILS

            // **************************
        }

        private static ShortKey Getkey(List<ShortKey> keys, ShortKey shortKey)
        {
            return keys.Where(s => s.Key == shortKey.Key && s.ModifierKeyFirst == shortKey.ModifierKeyFirst && s.ModifierKeySecond == shortKey.ModifierKeySecond).FirstOrDefault();
        }

        #endregion Methods
    }
}
