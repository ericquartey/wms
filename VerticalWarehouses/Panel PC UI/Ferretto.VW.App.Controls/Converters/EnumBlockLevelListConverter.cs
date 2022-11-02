using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using DevExpress.XtraRichEdit.Import.Rtf;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class EnumBlockLevelListConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //var list = value as IEnumerable<BlockLevel>;
            List<string> translate = new List<string>();
            foreach (var blk in value as IEnumerable<BlockLevel>)
            {
                switch (blk)
                {
                    case BlockLevel.None:
                        translate.Add(Resources.Localized.Get("OperatorApp.BlockLevelNone"));
                        break;

                    case BlockLevel.Undefined:
                        translate.Add(Resources.Localized.Get("OperatorApp.BlockLevelUndefined"));
                        break;

                    case BlockLevel.SpaceOnly:
                        translate.Add(Resources.Localized.Get("OperatorApp.BlockLevelSpaceOnly"));
                        break;

                    case BlockLevel.Blocked:
                        translate.Add(Resources.Localized.Get("OperatorApp.BlockLevelBlocked"));
                        break;

                    case BlockLevel.NeedsTest:
                        translate.Add(Resources.Localized.Get("OperatorApp.BlockLevelNeedsTest"));
                        break;

                    case BlockLevel.UnderWeight:
                        translate.Add(Resources.Localized.Get("OperatorApp.BlockLevelUnderWeight"));
                        break;

                    case BlockLevel.Reserved:
                        translate.Add(Resources.Localized.Get("OperatorApp.BlockLevelReserved"));
                        break;

                    default:
                        translate.Add(((BlockLevel)value).ToString());
                        break;
                }
            }
            return translate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
