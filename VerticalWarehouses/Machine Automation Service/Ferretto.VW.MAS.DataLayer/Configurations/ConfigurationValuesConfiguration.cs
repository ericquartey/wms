using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class ConfigurationValuesConfiguration : IEntityTypeConfiguration<ConfigurationValue>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<ConfigurationValue> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(cv => new { cv.CategoryName, cv.VarName });
        }

        #endregion
    }
}
