using System.Linq;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.DataModels;

namespace Ferretto.WMS.Data.Core.Extensions
{
    public static class IModelExtension
    {
        #region Methods

        public static string ValidateBusinessModel<TDataModel, TKey>(this IModel<TKey> model, IQueryable<TDataModel> dbSet)
            where TDataModel : class, IDataModel<TKey>
        {
            var uniqueProperties = model.GetType()
                .GetProperties().Where(
                    p => p.CustomAttributes.Any(
                        a => a.AttributeType == typeof(UniqueAttribute)))
                .ToArray();

            foreach (var uniqueProperty in uniqueProperties)
            {
                var businessModelPropertyValue = uniqueProperty.GetValue(model);
                var dataModelProperty = typeof(TDataModel).GetProperty(uniqueProperty.Name);

                if (dataModelProperty != null &&
                    dbSet.Any(
                        m => !m.Id.Equals(model.Id) &&
                            businessModelPropertyValue != null &&
                            businessModelPropertyValue.Equals(dataModelProperty.GetValue(m))))
                {
                    return string.Format(
                        Resources.Errors.TheValueForFieldIsAlreadyUsedByAnotherEntry,
                        businessModelPropertyValue,
                        uniqueProperty.Name);
                }
            }

            return null;
        }

        #endregion
    }
}
