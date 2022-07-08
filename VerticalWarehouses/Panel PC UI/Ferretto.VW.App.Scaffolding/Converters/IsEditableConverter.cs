using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using CommonServiceLocator;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    public class IsEditableConverter : IValueConverter
    {
        #region Fields

        private readonly ISessionService sessionService = ServiceLocator.Current.GetInstance<ISessionService>();
        private readonly IMachineService machineService = ServiceLocator.Current.GetInstance<IMachineService>();

        #endregion

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.ScaffoldedEntity entity)
            {
                // detour to the 'Metadata'
                value = entity.Metadata;

                if (entity.Property.Name == "FireAlarm" && this.machineService.Bays.Any(s => s.IsExternal && s.IsDouble))
                {
                    return false;
                }
                if (entity.Property.Name == "ChainOffset" && this.machineService.Bays.Any(s => s.IsExternal) && this.sessionService.UserAccessLevel == UserAccessLevel.Installer)
                {
                    return true;
                }
                if ((entity.Property.Name == "Center" || entity.Property.Name == "Race") && this.sessionService.UserAccessLevel == UserAccessLevel.Installer)
                {
                    return true;
                }
                if ((entity.Instance is Shutter ||
                    entity.Instance is ShutterManualParameters) &&
                    this.sessionService.UserAccessLevel >= UserAccessLevel.Installer)
                {
                    return true;
                }
                if ((entity.Instance is BayPosition) && entity.Property.Name == "IsBlocked" &&
                    this.sessionService.UserAccessLevel >= UserAccessLevel.Installer)
                {
                    return true;
                }
            }

            if (value is IEnumerable<Attribute> metadata)
            {
                var editable = metadata.OfType<EditableAttribute>().FirstOrDefault();
                if (editable != null)
                {
                    if (this.sessionService.UserAccessLevel == UserAccessLevel.Admin || this.sessionService.UserAccessLevel == UserAccessLevel.Support)
                    {
                        return editable.AllowEdit;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (this.sessionService.UserAccessLevel == UserAccessLevel.Admin || this.sessionService.UserAccessLevel == UserAccessLevel.Support)
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
