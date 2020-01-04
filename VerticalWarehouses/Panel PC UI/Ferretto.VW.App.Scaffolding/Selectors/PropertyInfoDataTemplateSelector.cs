using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Scaffolding.Selectors
{
    public class PropertyInfoDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement element && item is Models.ScaffoldedEntity entity)
            {
                var metadata = entity.Metadata;

                string resourceKey = default;

                UIHintAttribute uiHint = metadata.OfType<UIHintAttribute>().FirstOrDefault();
                if (uiHint != null)
                {
                    // TODO: custom stuff
                }

                // try with datatype
                if (string.IsNullOrEmpty(resourceKey))
                {
                    DataTypeAttribute dataType = metadata.OfType<DataTypeAttribute>().FirstOrDefault();
                    if (dataType != null)
                    {
                        switch (dataType.DataType)
                        {
                            case DataType.Password:
                                resourceKey = "PasswordDataTemplate";
                                break;
                            case DataType.EmailAddress:
                                resourceKey = "EmailDataTemplate";
                                break;
                        }
                    }
                }

                // try with very type
                if (string.IsNullOrEmpty(resourceKey))
                {
                    Type type = entity.Property.PropertyType;
                    if (type == typeof(System.Net.IPAddress))
                    {
                        resourceKey = "IPAddressDataTemplate";
                    }
                }

                return element.FindResource(resourceKey ?? /* fallback */ "StringDataTemplate") as DataTemplate;
            }

            return null;
        }
    }
}
