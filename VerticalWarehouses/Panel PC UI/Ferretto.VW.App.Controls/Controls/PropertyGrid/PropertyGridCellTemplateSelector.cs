using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.PropertyGrid;

namespace Ferretto.VW.App.Controls
{
    public class PropertyGridCellTemplateSelector : DataTemplateSelector
    {
        #region Fields

        private const string TEMPLATE_ID_READONLY = "PropDefReadonlyIDTemplate";

        private const string TEMPLATE_NAME = "PropDef{0}Template";

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var editor = container as CellEditor;
            var rowData = item as RowData;
            var propertyGrid = editor.PropertyGrid;

            string propertyName;
            object parentObject;
            var fullPath = rowData.FullPath;
            if (fullPath.Contains("."))
            {
                propertyName = fullPath.Substring(fullPath.LastIndexOf(".") + 1);
                parentObject = propertyGrid.GetRowValueByRowPath(fullPath.Substring(0, fullPath.LastIndexOf(".")));
            }
            else
            {
                propertyName = fullPath;
                parentObject = propertyGrid.SelectedObject;
            }
            var property = TypeDescriptor.GetProperties(parentObject).Cast<PropertyDescriptor>().Where(x => x.Name == propertyName).FirstOrDefault();
            if (property == null
                ||
                propertyName == null)
            {
                return null;
            }

            if (property.Name.EndsWith("Id"))
            {
                return this.GetDataTemplate(TEMPLATE_ID_READONLY);
            }

            if (property.IsReadOnly)
            {
                return this.GetDataTemplate(TEMPLATE_ID_READONLY);
            }

            if (property.PropertyType is Type type)

            {
                if (type.IsValueType
                    ||
                    type.Name == typeof(IPAddress).Name)
                {
                    var templateName = string.Format(TEMPLATE_NAME, type.Name);
                    return this.GetDataTemplate(templateName);
                }
            }

            return null;
        }

        private DataTemplate GetDataTemplate(string templateName)
        {
            return Application.Current.TryFindResource(templateName) as DataTemplate;
        }

        #endregion
    }
}
