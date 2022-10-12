using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Scaffolding.Selectors
{
    public class PropertyInfoDataTemplateSelector : DataTemplateSelector
    {
        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement element && item is Models.ScaffoldedEntity entity)
            {
                var metadata = entity.Metadata;

                string resourceKey = default;

                var uiHint = metadata.OfType<UIHintAttribute>().FirstOrDefault();
                if (uiHint != null)
                {
                    // TODO: custom stuff
                }

                // try with (enum)datatype
                if (string.IsNullOrEmpty(resourceKey))
                {
                    if (entity.Property.PropertyType.IsEnum || metadata.OfType<EnumDataTypeAttribute>().Any())
                    {
                        resourceKey = "EnumDataTemplate";
                    }
                }

                // try with datatype
                if (string.IsNullOrEmpty(resourceKey))
                {
                    var dataType = metadata.OfType<DataTypeAttribute>().FirstOrDefault();
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
                    var type = entity.Property.PropertyType;
                    if (type == typeof(System.Net.IPAddress))
                    {
                        resourceKey = "IPAddressDataTemplate";
                    }
                    else if (this.IsNumeric(type))
                    {
                        if (type.FullName == "System.Int32" || type == typeof(int?))
                        {
                            resourceKey = "NumericIntDataTemplate";
                        }
                        else
                        {
                            resourceKey = "NumericDoubleDataTemplate";
                        }
                    }
                    else if (this.IsBoolean(type))
                    {
                        resourceKey = "BooleanDataTemplate";
                    }
                }

                return element.FindResource(resourceKey ?? /* fallback */ "StringDataTemplate") as DataTemplate;
            }

            return null;
        }

        private bool IsBoolean(Type complexType)
        {
            var type = Nullable.GetUnderlyingType(complexType) ?? complexType;
            return type == typeof(bool);
        }

        private bool IsNumeric(Type complexType)
        {
            var type = Nullable.GetUnderlyingType(complexType) ?? complexType;
            return type == typeof(byte)
                       || type == typeof(short)
                       || type == typeof(ushort)
                       || type == typeof(int)
                       || type == typeof(uint)
                       || type == typeof(long)
                       || type == typeof(ulong)
                       || type == typeof(float)
                       || type == typeof(double)
                       || type == typeof(decimal);
        }

        #endregion
    }
}
