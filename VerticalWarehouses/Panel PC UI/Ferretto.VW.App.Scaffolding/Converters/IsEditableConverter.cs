using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Ferretto.VW.App.Modules.Login;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    public class IsEditableConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.ScaffoldedEntity entity)
            {
                // detour to the 'Metadata'
                value = entity.Metadata;


                if (entity.Property.Name == "Center" && ScaffolderUserAccesLevel.User == MAS.AutomationService.Contracts.UserAccessLevel.Installer)
                {
                    return true;
                }
            }

            if (value is IEnumerable<Attribute> metadata)
            {
                var editable = metadata.OfType<EditableAttribute>().FirstOrDefault();
                if (editable != null)
                {
                    if (ScaffolderUserAccesLevel.User == MAS.AutomationService.Contracts.UserAccessLevel.Admin || ScaffolderUserAccesLevel.User == MAS.AutomationService.Contracts.UserAccessLevel.Support)
                    {
                        return editable.AllowEdit;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (ScaffolderUserAccesLevel.User == MAS.AutomationService.Contracts.UserAccessLevel.Admin || ScaffolderUserAccesLevel.User == MAS.AutomationService.Contracts.UserAccessLevel.Support)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
